using App.Data;
using App.Models;
using App.Models.Products;
using App.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace App.Areas.Products.Controllers
{
    [Area("Products")]
    [Route("/OrderManager/[action]")]
    [Authorize(Roles = RoleName.Administrator)]
    public class OrderController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ProductController> _logger;
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailSender _emailSender;

        private readonly int ITEM_PER_PAGE = 10;

        [TempData]
        public string? StatusMessage { set; get; }

        public OrderController(IEmailSender emailSender, UserManager<AppUser> userManager, ILogger<ProductController> logger, AppDbContext context)
        {
            _emailSender = emailSender;
            _userManager = userManager;
            _logger = logger;
            _context = context;
        }

        // Trang thống kê
        public IActionResult Index([FromQuery(Name = "p")]int currentPage,[FromQuery(Name = "s")]string? searchString, [FromQuery(Name = "f")] int? filter)
        {
            var order = _context.Orders
                                .OrderByDescending(o => o.OrderDate)
                                .Include(o => o.OrderStatuses.OrderBy(os => os.DateUpdate))
                                .Include(o => o.User)
                                .AsQueryable();

            if (searchString != null)
            {
                searchString = searchString.ToLower();
                order = order.Where(o => o.FullName.ToLower().Contains(searchString) || o.Code.ToLower().Contains(searchString) || o.Email.ToLower().Contains(searchString));
            }

            if (filter != null)
            {
                switch (filter)
                {
                    case -1:
                        order = order.Where(o => !o.PayStatuses.Any(ps => ps.ResponseCode == "00") && !o.OrderStatuses.Any(od => od.Code == (int)OrderStatusCode.Canceled));
                        break;
                    case 0:
                        order = order.Where(o => o.OrderStatuses.OrderBy(os => os.DateUpdate).Last().Code == (int)OrderStatusCode.WaitAccept);
                        break;
                    case 1:
                        order = order.Where(o => o.OrderStatuses.OrderBy(os => os.DateUpdate).Last().Code == (int)OrderStatusCode.Accepted);
                        break;
                    case 2:
                        order = order.Where(o => o.OrderStatuses.OrderBy(os => os.DateUpdate).Last().Code == (int)OrderStatusCode.Delivering);
                        break;
                    case 3:
                        order = order.Where(o => o.OrderStatuses.OrderBy(os => os.DateUpdate).Last().Code == (int)OrderStatusCode.Delivered);
                        break;
                    case 4:
                        order = order.Where(o => o.OrderStatuses.OrderBy(os => os.DateUpdate).Last().Code == (int)OrderStatusCode.Canceled);
                        break;
                    default:
                    
                        break;
                }
            }


            int totalPage = (int)Math.Ceiling((decimal)order.Count() / ITEM_PER_PAGE);
            if (totalPage < 1) totalPage = 1;

            if (currentPage < 1) currentPage = 1;
            if (currentPage > totalPage) currentPage = totalPage;

            ViewBag.CurrentPage = currentPage;
            ViewBag.TotalPage = totalPage;

            var orderInPage = order.Skip((currentPage - 1) * ITEM_PER_PAGE).Take(ITEM_PER_PAGE).ToList();
            ViewBag.TotalOrders = orderInPage.Count;

            return View(orderInPage);
        }

        [Route("/user/order")]
        [Authorize(Roles = null)]
        public async Task<IActionResult> OrderByUser(int? status)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var order = _context.Orders.Where(o => o.UserId == user.Id)
                                        .Include(o => o.OrderDetails)
                                        .ThenInclude(o => o.Product)
                                        .Include(o => o.OrderDetails)
                                        .ThenInclude(o => o.Color)
                                        .Include(o => o.OrderDetails)
                                        .ThenInclude(o => o.Capacity)
                                        .Include(o => o.OrderStatuses.OrderBy(os => os.DateUpdate))
                                        .Include(o => o.PayStatuses)
                                        .OrderByDescending(o => o.OrderDate)
                                        .AsSplitQuery();

            if (status != null)
            {
                if (status == -1)
                {
                    order = order.Where(o => o.PayType == "Online" && !o.PayStatuses.Any(p => p.ResponseCode == "00"));
                }
                else
                {
                    order = order.Where(o => o.OrderStatuses.OrderBy(os => os.DateUpdate).Last().Code == status);
                }
            }

            return View(order.ToList());
        }

        // Trang chi tiết
        [HttpGet("{Id}")]
        public IActionResult Details(int Id)
        {
            var order = _context.Orders.Where(o => o.Id == Id)
                                .Include(o => o.OrderDetails)
                                .Include(o => o.OrderStatuses)
                                .Include(o => o.PayStatuses)
                                .AsSplitQuery()
                                .FirstOrDefault();

            if (order == null) return NotFound();
            order.OrderStatuses = order.OrderStatuses.OrderBy(s => s.DateUpdate).ToList();
            order.OrderDetails.ForEach(e =>
            {
                e.Product = _context.Products.FirstOrDefault(p => p.Id == e.ProductId);
                e.Color = _context.Colors.FirstOrDefault(c => c.Id == e.ColorId);
                e.Capacity = _context.Capacities.FirstOrDefault(c => c.Id == e.CapacityId);
            });

            return View(order);
        }

        // Xác nhận đơn hàng
        public async Task<IActionResult> AcceptOrder(int Id)
        {
            var order = _context.Orders.Where(o => o.Id == Id)
                                .Include(o => o.OrderStatuses.OrderBy(o => o.DateUpdate))
                                .Include(o => o.PayStatuses)
                                .AsSplitQuery()
                                .FirstOrDefault();

            if (order == null) return NotFound();

            order.OrderStatuses = order.OrderStatuses;

            if (order.PayType == "Online")
            {
                if (!order.PayStatuses.Any(p => p.ResponseCode == "00"))
                {
                    StatusMessage = "Đơn hàng này chưa được thanh toán, không được xác nhận. Hãy hủy nếu sau thời gian dài không thanh toán";
                    return RedirectToAction(nameof(Details), new { Id });
                }
            }

            if (order.OrderStatuses.Last().Status != OrderStatuses.WaitAccept)
            {
                StatusMessage = "Trạng thái trước đó không hợp lệ";
                return RedirectToAction(nameof(Details), new { Id });
            }

            var user = await _userManager.GetUserAsync(User);

            order.OrderStatuses.Add(new OrderStatus()
            {
                Code = (int)OrderStatusCode.Accepted,
                DateUpdate = DateTime.Now,
                Status = OrderStatuses.Accepted,
                UserId = user?.Id,
                Note = $"Đơn hàng đã được xác nhận",

            });
            await _context.SaveChangesAsync();

            string emailContent =
$@"
Thông báo, đơn hàng của bạn đã được xác nhận. Chúng tôi đang chuẩn bị đơn hàng giao cho đơn vị vận chuyển.
Vui lòng theo dõi đơn hàng trong mục Theo dõi đơn hàng hoặc <a href='{Url.Action("OrderCheck", "ViewProduct", new { area = "Products", PhoneNumber = order.PhoneNumber, Code = order.Code }, HttpContext.Request.Scheme, HttpContext.Request.Host.Value)}'>nhấn vào đây</a>.

Mã đơn hàng: {order.Code}

Mọi thông tin chi tiết vui lòng liên hệ 1800.1789
Xin cảm ơn.";

            string emailHtml = AppUtilities.GenerateHtmlEmail(order.FullName, emailContent);

            await _emailSender.SendEmailAsync(order.Email, "Đơn hàng đã được xác nhận", emailHtml);

            return RedirectToAction(nameof(Details), new { Id });
        }

        // Gửi đơn hàng
        public async Task<IActionResult> Delivery(int Id)
        {
            var order = _context.Orders.Where(o => o.Id == Id)
                                .Include(o => o.OrderStatuses)
                                .Include(o => o.OrderDetails)
                                    .ThenInclude(p => p.Product)
                                .Include(o => o.OrderDetails)
                                    .ThenInclude(p => p.Color)
                                .Include(o => o.OrderDetails)
                                    .ThenInclude(p => p.Capacity)
                                .AsSplitQuery()
                                .FirstOrDefault();

            if (order == null) return NotFound();

            order.OrderStatuses = order.OrderStatuses.OrderBy(o => o.DateUpdate).ToList();
            if (order.OrderStatuses.Last().Status != OrderStatuses.Accepted)
            {
                StatusMessage = "Trạng thái trước đó không hợp lệ";
                return RedirectToAction(nameof(Details), new { Id });
            }

            var user = await _userManager.GetUserAsync(User);

            var dateTimeNow = DateTime.Now;

            order.OrderStatuses.Add(new OrderStatus()
            {
                Code = (int)OrderStatusCode.Delivering,
                DateUpdate = dateTimeNow,
                Status = OrderStatuses.Delivering,
                UserId = user?.Id,
                Note = $"Đơn hàng đã được gửi cho vận chuyển.",

            });

            order.OrderDetails.ForEach(item =>
            {
                if (item.Capacity != null)
                {
                    item.Capacity.Quantity -= item.Quantity;
                }
            });

            await _context.SaveChangesAsync();

            string emailContent =
$@"
Thông báo, đơn hàng của bạn đã được giao cho bên vận chuyển - {dateTimeNow.ToString("hh:mm dd/MM/yyy")}.
Vui lòng theo dõi đơn hàng trong mục Theo dõi đơn hàng hoặc <a href='{Url.Action("OrderCheck", "ViewProduct", new { area = "Products", PhoneNumber = order.PhoneNumber, Code = order.Code }, HttpContext.Request.Scheme, HttpContext.Request.Host.Value)}'>nhấn vào đây</a>.

Mã đơn hàng: {order.Code}

Mọi thông tin chi tiết vui lòng liên hệ 1800.1789
Xin cảm ơn.";

            string emailHtml = AppUtilities.GenerateHtmlEmail(order.FullName, emailContent);

            await _emailSender.SendEmailAsync(order.Email, "Đơn hàng đang được giao", emailHtml);



            return RedirectToAction(nameof(Details), new { Id });
        }

        // Đã giao
        public async Task<IActionResult> Delivered(int Id)
        {
            var order = _context.Orders.Where(o => o.Id == Id)
                                .Include(o => o.OrderStatuses)
                                .Include(o => o.OrderDetails)
                                    .ThenInclude(p => p.Product)
                                .Include(o => o.OrderDetails)
                                    .ThenInclude(p => p.Color)
                                .Include(o => o.OrderDetails)
                                    .ThenInclude(p => p.Capacity)
                                .AsSplitQuery()
                                .FirstOrDefault();

            if (order == null) return NotFound();

            order.OrderStatuses = order.OrderStatuses.OrderBy(o => o.DateUpdate).ToList();
            if (order.OrderStatuses.Last().Status != OrderStatuses.Delivering)
            {
                StatusMessage = "Trạng thái trước đó không hợp lệ";
                return RedirectToAction(nameof(Details), new { Id });
            }

            var user = await _userManager.GetUserAsync(User);

            var dateTimeNow = DateTime.Now;

            order.OrderStatuses.Add(new OrderStatus()
            {
                Code = (int)OrderStatusCode.Delivered,
                DateUpdate = dateTimeNow,
                Status = OrderStatuses.Delivered,
                UserId = user?.Id,
                Note = $"Đơn hàng đã được giao cho khách hàng.",
            });

            order.OrderDetails.ForEach(item =>
            {
                if (item.Capacity != null)
                    item.Capacity.Sold += item.Quantity;
            });

            var payStatus = new PayStatus() {
                Content = "Đã trả tiền khi nhận hàng",
                ResponseCode = "00",
                Date = DateTime.Now,
                CardType = "COD"
            };

            order.PayStatuses.Add(payStatus);

            await _context.SaveChangesAsync();

            string emailContent =
$@"
Thông báo, đơn hàng của bạn đã được giao thành công vào lúc {dateTimeNow.ToString("hh:mm dd/MM/yyy")}.
Cảm ơn bạn đã tin tưởng vào sản phẩm của chúng tôi.


Mọi thông tin chi tiết vui lòng liên hệ 1800.1789.
Xin cảm ơn.";

            string emailHtml = AppUtilities.GenerateHtmlEmail(order.FullName, emailContent);

            await _emailSender.SendEmailAsync(order.Email, "Đơn hàng giao thành công", emailHtml);

            return RedirectToAction(nameof(Details), new { Id });
        }

        [HttpPost]
        [Authorize(Roles = null)]
        public async Task<IActionResult> CancelOrderAPI(int Id, string? note)
        {
            var order = _context.Orders.Where(o => o.Id == Id)
                                .Include(o => o.OrderStatuses)
                                .Include(o => o.PayStatuses)
                                .Include(o => o.OrderDetails)
                                    .ThenInclude(p => p.Product)
                                .Include(o => o.OrderDetails)
                                    .ThenInclude(p => p.Color)
                                .Include(o => o.OrderDetails)
                                    .ThenInclude(p => p.Capacity)
                                .AsSplitQuery()
                                .FirstOrDefault();

            if (order == null) 
                return Json(new {
                    status = 0,
                    message = "Không tìm thấy đơn hàng này"
                });

            order.OrderStatuses = order.OrderStatuses.OrderBy(o => o.DateUpdate).ToList();

            if (order.OrderStatuses.Last().Status == OrderStatuses.Delivered)
            {
                return Json(new {
                    status = 0,
                    message = "Đơn hàng đã giao thành công. Không thể hủy."
                });
            }

            if (order.OrderStatuses.Last().Status == OrderStatuses.Canceled)
            {
                return Json(new {
                    status = 0,
                    message = "Đơn hàng đã được hủy từ trước rồi"
                });
            }

            if (order.PayStatuses.Any(p => p.ResponseCode == "00"))
            {
                return Json(new {
                    status = 0,
                    message = "Chưa hỗ trợ chức năng hoàn tiền, không thể hủy."
                });
            }

            var user = await _userManager.GetUserAsync(User);

            var dateTimeNow = DateTime.Now;

            if (order.OrderStatuses.Last().Status == OrderStatuses.Delivering)
            {
                order.OrderDetails.ForEach(item =>
                {
                    _logger.LogInformation("ABC");
                    if (item.Capacity != null)
                        item.Capacity.Quantity += item.Quantity;
                });
            }

            order.OrderStatuses.Add(new OrderStatus()
            {
                Code = (int)OrderStatusCode.Canceled,
                DateUpdate = dateTimeNow,
                Status = OrderStatuses.Canceled,
                UserId = user?.Id,
                Note = note,
            });

            await _context.SaveChangesAsync();

            string emailContent =
$@"
Thông báo, đơn hàng của bạn đã bị hủy.

Lý do: {note}

Mã đơn hàng: {order.Code}

Mọi thông tin chi tiết vui lòng liên hệ 1800.1789.
Xin cảm ơn.";

            string emailHtml = AppUtilities.GenerateHtmlEmail(order.FullName, emailContent);

            await _emailSender.SendEmailAsync(order.Email, "Hủy đơn hàng", emailHtml);
            
            return Json(new {
                status = 1,
                message = "Đã hủy đơn hàng"
            });;
        }

        // Hủy
        [HttpPost]
        public async Task<IActionResult> CancelOrder(int Id, string? note)
        {
            var order = _context.Orders.Where(o => o.Id == Id)
                                .Include(o => o.OrderStatuses)
                                .Include(o => o.PayStatuses)
                                .Include(o => o.OrderDetails)
                                    .ThenInclude(p => p.Product)
                                .Include(o => o.OrderDetails)
                                    .ThenInclude(p => p.Color)
                                .Include(o => o.OrderDetails)
                                    .ThenInclude(p => p.Capacity)
                                .AsSplitQuery()
                                .FirstOrDefault();

            if (order == null) return NotFound();

            order.OrderStatuses = order.OrderStatuses.OrderBy(o => o.DateUpdate).ToList();

            if (order.OrderStatuses.Last().Status == OrderStatuses.Delivered)
            {
                StatusMessage = "Đơn hàng đã giao thành công. Không thể hủy.";
                return RedirectToAction(nameof(Details), new { Id });
            }

            if (order.OrderStatuses.Last().Status == OrderStatuses.Canceled)
            {
                StatusMessage = "Đơn hàng đã được hủy từ trước rồi";
                return RedirectToAction(nameof(Details), new { Id });
            }

            if (order.PayStatuses.Any(p => p.ResponseCode == "00"))
            {
                StatusMessage = "Chưa hỗ trợ chức năng hoàn tiền, không thể hủy";
                return RedirectToAction(nameof(Details), new { Id });
            }

            var user = await _userManager.GetUserAsync(User);

            var dateTimeNow = DateTime.Now;

            if (order.OrderStatuses.Last().Status == OrderStatuses.Delivering)
            {
                order.OrderDetails.ForEach(item =>
                {
                    _logger.LogInformation("ABC");
                    if (item.Capacity != null)
                        item.Capacity.Quantity += item.Quantity;
                });
            }

            order.OrderStatuses.Add(new OrderStatus()
            {
                Code = (int)OrderStatusCode.Canceled,
                DateUpdate = dateTimeNow,
                Status = OrderStatuses.Canceled,
                UserId = user?.Id,
                Note = note,
            });

            await _context.SaveChangesAsync();

            string emailContent =
$@"
Thông báo, đơn hàng của bạn đã bị hủy.

Lý do: {note}

Mã đơn hàng: {order.Code}

Mọi thông tin chi tiết vui lòng liên hệ 1800.1789.
Xin cảm ơn.";

            string emailHtml = AppUtilities.GenerateHtmlEmail(order.FullName, emailContent);

            await _emailSender.SendEmailAsync(order.Email, "Hủy đơn hàng", emailHtml);
            return RedirectToAction(nameof(Details), new { Id });
        }
    }
}