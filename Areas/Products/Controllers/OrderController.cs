using App.Data;
using App.Models;
using App.Models.Products;
using App.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace App.Areas.Products.Controllers
{
    [Area("Products")]
    [Route("/OrderManager/[action]")]
    public class OrderController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ProductController> _logger;
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailSender _emailSender;

        [TempData]
        public string? StatusMessage {set;get;}

        public OrderController(IEmailSender emailSender, UserManager<AppUser> userManager, ILogger<ProductController> logger, AppDbContext context)
        {
            _emailSender = emailSender;
            _userManager = userManager;
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            var order = _context.Orders
                                .Include(o => o.OrderStatuses)
                                .ToList();

            order.ForEach(o => {
                o.OrderStatuses = o.OrderStatuses.OrderBy(s => s.DateUpdate).ToList();
            });
            
            return View(order);
        }

        [HttpGet("{Id}")]
        public IActionResult Details(int Id)
        {
            var order = _context.Orders.Where(o => o.Id == Id)
                                .Include(o => o.OrderDetails)
                                .Include(o => o.OrderStatuses)
                                .AsSplitQuery()
                                .FirstOrDefault();

            if (order == null)  return NotFound();
            order.OrderStatuses = order.OrderStatuses.OrderBy(s => s.DateUpdate).ToList();
            order.OrderDetails.ForEach(e => {
                e.Product = _context.Products.FirstOrDefault(p => p.Id == e.ProductId);
                e.Color = _context.Colors.FirstOrDefault(c => c.Id == e.ColorId);
                e.Capacity = _context.Capacities.FirstOrDefault(c => c.Id == e.CapacityId);
            });

            return View(order);
        }

        public async Task<IActionResult> AcceptOrder(int Id)
        {
            var order = _context.Orders.Where(o => o.Id == Id)
                                .Include(o => o.OrderStatuses)
                                .AsSplitQuery()
                                .FirstOrDefault();

            if (order == null)  return NotFound();     

            order.OrderStatuses = order.OrderStatuses.OrderBy(o => o.DateUpdate).ToList();
            if(order.OrderStatuses.Last().Status != OrderStatuses.WaitAccept)
            {
                StatusMessage = "Trạng thái trước đó không hợp lệ";
                return RedirectToAction(nameof(Details), new {Id});
            }

            var user = await _userManager.GetUserAsync(User);

            order.OrderStatuses.Add(new OrderStatus() {
                Code = (int)OrderStatusCode.Accepted,
                DateUpdate = DateTime.Now,
                Status = OrderStatuses.Accepted,
                UserId = user?.Id,
                Note = $"Đơn hàng đã được xác nhận",

            });
            await _context.SaveChangesAsync();

            string emailContent = 
$@"
Thông báo, đơn hàng của bạn đã được xác nhận. Chúng tôi đang chuẩn bị đơn hàng giao cho đơn vị vận chuyển 
Vui lòng theo dõi đơn hàng trong mục Theo dõi đơn hàng.

Mã đơn hàng: {order.Code}

Mọi thông tin chi tiết vui lòng liên hệ 1800.1789
Xin cảm ơn.";

            string emailHtml = AppUtilities.GenerateHtmlEmail(order.FullName, emailContent);

            await _emailSender.SendEmailAsync(order.Email, "Đơn hàng đã được xác nhận", emailHtml);

            return RedirectToAction(nameof(Details), new {Id});
        }

        public async Task<IActionResult> Delivery(int Id)
        {
            var order = _context.Orders.Where(o => o.Id == Id)
                                .Include(o => o.OrderStatuses)
                                .AsSplitQuery()
                                .FirstOrDefault();

            if (order == null)  return NotFound();     

            order.OrderStatuses = order.OrderStatuses.OrderBy(o => o.DateUpdate).ToList();
            if(order.OrderStatuses.Last().Status != OrderStatuses.Accepted)
            {
                StatusMessage = "Trạng thái trước đó không hợp lệ";
                return RedirectToAction(nameof(Details), new {Id});
            }

            var user = await _userManager.GetUserAsync(User);

            var dateTimeNow = DateTime.Now;

            order.OrderStatuses.Add(new OrderStatus() {
                Code = (int)OrderStatusCode.Delivering,
                DateUpdate = dateTimeNow,
                Status = OrderStatuses.Delivering,
                UserId = user?.Id,
                Note = $"Đơn hàng được gửi cho vận chuyển.",

            });
            await _context.SaveChangesAsync();

            string emailContent = 
$@"
Thông báo, đơn hàng của bạn đã được giao cho bên vận chuyển - {dateTimeNow.ToString("hh:mm dd/MM/yyy")}.
Vui lòng theo dõi đơn hàng trong mục Theo dõi đơn hàng.

Mã đơn hàng: {order.Code}

Mọi thông tin chi tiết vui lòng liên hệ 1800.1789
Xin cảm ơn.";

            string emailHtml = AppUtilities.GenerateHtmlEmail(order.FullName, emailContent);

            await _emailSender.SendEmailAsync(order.Email, "Đơn hàng đang được giao", emailHtml);

            return RedirectToAction(nameof(Details), new {Id});
        }

        public async Task<IActionResult> Delivered(int Id)
        {
            var order = _context.Orders.Where(o => o.Id == Id)
                                .Include(o => o.OrderStatuses)
                                .AsSplitQuery()
                                .FirstOrDefault();

            if (order == null)  return NotFound();     

            order.OrderStatuses = order.OrderStatuses.OrderBy(o => o.DateUpdate).ToList();
            if(order.OrderStatuses.Last().Status != OrderStatuses.Delivering)
            {
                StatusMessage = "Trạng thái trước đó không hợp lệ";
                return RedirectToAction(nameof(Details), new {Id});
            }

            var user = await _userManager.GetUserAsync(User);

            var dateTimeNow = DateTime.Now;

            order.OrderStatuses.Add(new OrderStatus() {
                Code = (int)OrderStatusCode.Delivered,
                DateUpdate = dateTimeNow,
                Status = OrderStatuses.Delivered,
                UserId = user?.Id,
                Note = $"Đơn hàng đã được giao cho khách hàng.",
            });

            await _context.SaveChangesAsync();

            string emailContent = 
$@"
Thông báo, đơn hàng của bạn đã được giao thành công vào lúc {dateTimeNow.ToString("hh:mm dd/MM/yyy")}.
Cảm ơn bạn đã tin tưởng vào sản phẩm của chúng tôi.


Mọi thông tin chi tiết vui lòng liên hệ 1800.1789.
Xin cảm ơn.";

            string emailHtml = AppUtilities.GenerateHtmlEmail(order.FullName, emailContent);

            await _emailSender.SendEmailAsync(order.Email, "Đơn hàng giao thành công", emailHtml);

            return RedirectToAction(nameof(Details), new {Id});
        }

        [HttpPost]
        public async Task<IActionResult> CancelOrder(int Id, string? note)
        {
            var order = _context.Orders.Where(o => o.Id == Id)
                                .Include(o => o.OrderStatuses)
                                .AsSplitQuery()
                                .FirstOrDefault();

            if (order == null)  return NotFound();     

            order.OrderStatuses = order.OrderStatuses.OrderBy(o => o.DateUpdate).ToList();
            if(order.OrderStatuses.Last().Status == OrderStatuses.Delivered)
            {
                StatusMessage = "Trạng thái trước đó không hợp lệ";
                return RedirectToAction(nameof(Details), new {Id});
            }

            var user = await _userManager.GetUserAsync(User);

            var dateTimeNow = DateTime.Now;

            order.OrderStatuses.Add(new OrderStatus() {
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
            return RedirectToAction(nameof(Details), new {Id});
        }
    }
}