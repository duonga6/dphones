using System.Net.Mime;
using App.Areas.Products.Models;
using App.Areas.Products.Services;
using App.Data;
using App.Models;
using App.Models.Products;
using App.Models.VnPay;
using App.Services;
using App.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using App.Areas.Products.Models.Cart;

namespace App.Areas.Products.Controllers
{
    [Area("Products")]
    public class ViewProductController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ProductController> _logger;
        private readonly UserManager<AppUser> _userManager;
        private readonly CartService _cart;
        private readonly IEmailSender _emailSender;
        private readonly int ITEM_PER_PAGE = 9;
        private readonly VnPayService _vnPay;
        private readonly IHttpClientFactory _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        [TempData]
        public string? StatusMessage { set; get; }

        public ViewProductController(ILogger<ProductController> logger, AppDbContext context, CartService cart, UserManager<AppUser> userManager, IEmailSender emailSender, VnPayService vnPay, IHttpClientFactory httpClient, IConfiguration configuration, IWebHostEnvironment environment)
        {
            _logger = logger;
            _context = context;
            _cart = cart;
            _userManager = userManager;
            _emailSender = emailSender;
            _vnPay = vnPay;
            _httpClient = httpClient;
            _configuration = configuration;
            _environment = environment;
        }

        [Route("/dien-thoai")]
        public async Task<IActionResult> Index([FromQuery] string hangsx, [FromQuery] string danhmuc, [FromQuery] string mucgia, [FromQuery] string sort, [FromQuery(Name = "p")] int currentPage, [FromQuery(Name = "s")] string searchString)
        {
            var now = DateTime.Now;
            var products = _context.Products.Where(x => x.Published)
                                            .Include(p => p.Brand)
                                            .Include(p => p.ProductCategories)
                                                .ThenInclude(c => c.Category)
                                            .Include(p => p.Colors.OrderBy(c => c.Name))
                                                .ThenInclude(c => c.Capacities.OrderBy(ca => ca.SellPrice))
                                            .Include(p => p.Reviews)
                                            .Include(p => p.ProductDiscounts)
                                                .ThenInclude(x => x.Discount)
                                            .OrderByDescending(p => p.ReleaseDate)
                                            .AsNoTracking()
                                            .AsSplitQuery();

            var brands = hangsx?.Split(",", StringSplitOptions.RemoveEmptyEntries);
            var categories = danhmuc?.Split(",", StringSplitOptions.RemoveEmptyEntries);
            var price = mucgia?.Split("-", StringSplitOptions.RemoveEmptyEntries);

            if (searchString != null)
            {
                products = products.Where(p => p.Name.Contains(searchString));
            }

            if (brands != null)
            {
                products = products.Where(p => p.Brand != null && brands.Contains(p.Brand.Name));
            }

            if (categories != null)
            {
                var productCategoryId = _context.ProductCategories
                                    .AsNoTracking()
                                    .Include(p => p.Category)
                                    .Where(p => p.Category != null && categories.Contains(p.Category.Slug))
                                    .Select(p => p.ProductId);

                products = products.Where(p => productCategoryId.Contains(p.Id));
            }

            if (price != null)
            {
                _ = decimal.TryParse(price[0], out decimal tu);
                _ = decimal.TryParse(price[1], out decimal den);
                tu *= 1000000;
                den *= 1000000;
                try
                {
                    if (tu > den)
                        products = products.Where(p => p.Colors.First().Capacities.First().SellPrice >= tu);
                    else
                        products = products.Where(p => p.Colors.First().Capacities.First().SellPrice >= tu && p.Colors.First().Capacities.First().SellPrice <= den);
                }
                catch
                {

                }
            }

            switch (sort)
            {
                case "ngayramat":
                    products = products.OrderByDescending(p => p.ReleaseDate);
                    break;
                case "giathap":
                    products = products.OrderBy(p => p.Colors.SelectMany(c => c.Capacities).Min(c => c.SellPrice));
                    break;
                case "giacao":
                    products = products.OrderByDescending(p => p.Colors.SelectMany(c => c.Capacities).Min(c => c.SellPrice));
                    break;
                case "banchay":
                    products = products.OrderByDescending(p => p.Colors.SelectMany(c => c.Capacities).Max(c => c.Sold));
                    break;
                default:
                    products = products.OrderByDescending(p => p.ReleaseDate);
                    break;
            }



            ViewBag.Brands = await _context.Brands.ToListAsync();
            ViewBag.Categories = await _context.Categories.ToListAsync();
            ViewBag.PriceLevels = await _context.PriceLevels.OrderBy(p => p.Level).Select(p => p.Level).ToListAsync();
            ViewBag.TotalProduct = await products.CountAsync();

            int countPage = (int)Math.Ceiling((decimal)ViewBag.TotalProduct / ITEM_PER_PAGE);
            if (countPage < 1) countPage = 1;

            if (currentPage < 1) currentPage = 1;
            if (currentPage > countPage) currentPage = countPage;

            ViewBag.CountPage = countPage;
            ViewBag.CurrentPage = currentPage;


            var productInPage = products.Skip((currentPage - 1) * ITEM_PER_PAGE).Take(ITEM_PER_PAGE);

            return View(await productInPage.ToListAsync());
        }


        [Route("/{slug}")]
        public async Task<IActionResult> Details(string slug)
        {
            var product = await _context.Products.Where(x => x.Published).AsNoTracking()
                                            .Include(p => p.Brand)
                                            .Include(p => p.Photos)
                                            .Include(p => p.Colors)
                                                .ThenInclude(c => c.Capacities)
                                            .Include(p => p.ProductDiscounts)
                                                .ThenInclude(p => p.Discount)
                                            .AsSplitQuery()
                                            .FirstOrDefaultAsync(p => p.Slug == slug);

            if (product == null) return NotFound();

            product.Colors = product.Colors.OrderBy(c => c.Name).ToList();
            product.Colors.ForEach(c =>
            {
                c.Capacities = c.Capacities.OrderBy(ca => ca.Rom).ToList();
            });

            decimal currentPrice = product.Colors.First().Capacities.First().SellPrice;
            decimal rangePrice = 3000000.0m;

            var otherProduct = await _context.Products
                                                .Include(p => p.ProductDiscounts)
                                                    .ThenInclude(p => p.Discount)
                                                .Include(p => p.Colors.OrderBy(c => c.Name))
                                                    .ThenInclude(c => c.Capacities.OrderBy(ca => ca.SellPrice))
                                                .Include(p => p.Reviews)
                                                .Where(p => Math.Abs(p.Colors.First().Capacities.First().SellPrice - currentPrice) < rangePrice)
                                                .AsSingleQuery()
                                                .AsNoTracking()
                                                .Take(4)
                                                .ToListAsync();
            ViewBag.otherProducts = otherProduct;


            var posts = await _context.Posts.OrderByDescending(x => x.CreatedAt).Take(4).ToListAsync();
            ViewBag.Posts = posts;

            var user = await _userManager.GetUserAsync(User);
            ViewBag.User = user;

            return View(product);
        }

        [HttpGet]
        [Route("/get-color")]
        public async Task<IActionResult> GetColor(int productId)
        {
            var color = await _context.Colors.Where(c => c.ProductId == productId).OrderBy(c => c.Name).ToListAsync();

            return Ok(JsonConvert.SerializeObject(color));
        }

        [HttpGet]
        [Route("/get-capacity")]
        public async Task<IActionResult> GetCapacity(int colorId)
        {
            var capacity = await _context.Capacities.Where(c => c.ColorId == colorId).OrderBy(c => c.Rom).ToListAsync();
            return Ok(JsonConvert.SerializeObject(capacity));
        }


        [Route("/cart")]
        public IActionResult Cart()
        {
            return View();
        }


        [HttpPost]
        [Route("/cart/add-to-cart")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
        {
            var now = DateTime.Now;
            var product = await _context.Products.Where(x => x.Published).Include(p => p.ProductDiscounts.Where(x => x.Discount.StartAt <= now && x.Discount.EndAt >= now))
                                            .ThenInclude(x => x.Discount)
                                            .AsNoTracking()
                                            .FirstOrDefaultAsync(p => p.Id == request.productId);

            var color = await _context.Colors
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync(c => c.Id == request.colorId && c.ProductId == request.productId);

            var capa = await _context.Capacities
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync(c => c.Id == request.capaId && c.ColorId == request.colorId);

            if (product == null || color == null || capa == null)
            {
                return Json(new
                {
                    status = 0,
                    message = "Thêm thất bại. Không tìm thấy sản phẩm yêu cầu!"
                });
            }

            if (capa.Quantity < 1)
            {
                return Json(new
                {
                    status = 0,
                    message = "Sản phẩm này đã hết hàng"
                });
            }

            product.Description = null;

            var cartItem = new CartItem()
            {
                Id = Guid.NewGuid().ToString(),
                Product = product,
                Capacity = capa,
                Color = color,
                Quantity = 1
            };

            var cartList = _cart.GetCart();
            cartList ??= new List<CartItem>();
            var productInCart = cartList.FirstOrDefault(p => p.Product.Id == request.productId && p.Color.Id == request.colorId && p.Capacity.Id == request.capaId);
            if (productInCart == null)
            {
                cartList.Add(cartItem);
            }
            else
            {
                if (productInCart.Quantity >= productInCart.Capacity.Quantity)
                {
                    return Json(new
                    {
                        status = 0,
                        message = "Sản phẩm bạn chọn đã hết!"
                    });
                }
                else
                {
                    productInCart.Quantity++;
                }
            }

            _cart.SaveCart(cartList);

            return Json(new
            {
                status = 1,
                message = "Đã thêm vào giỏ hàng!",
                qtt = cartList.Count(),
                id = productInCart == null ? cartItem.Id : productInCart.Id
            });
        }

        [HttpPut]
        [Route("/cart/plus")]
        public async Task<IActionResult> PlusQuantityAsync(string Id)
        {
            var cartList = _cart.GetCart();
            var cartItem = cartList?.FirstOrDefault(c => c.Id == Id);
            var capa = await _context.Capacities.FindAsync(cartItem?.Capacity.Id);

            if (cartItem == null || capa == null)
            {
                return NotFound(new
                {
                    status = 0,
                    message = "Không tìm thấy sản phẩm này!"
                });
            }

            if (cartItem.Quantity >= capa.Quantity)
            {
                return Ok(new
                {
                    status = 0,
                    message = "Sản phẩm này đã hết!",
                    qtt = cartItem.Quantity
                });
            }
            else
            {
                cartItem.Quantity++;
                _cart.SaveCart(cartList);
                return Ok(new
                {
                    status = 1,
                    message = "Đã tăng số lượng!",
                    qtt = cartItem.Quantity
                });
            }
        }

        [HttpPut]
        [Route("/cart/minus")]
        public async Task<IActionResult> MinusQuantityAsync(string Id)
        {
            var cartList = _cart.GetCart();
            var cartItem = cartList?.FirstOrDefault(c => c.Id == Id);
            var capa = await _context.Capacities.FindAsync(cartItem?.Capacity.Id);

            if (cartItem == null || capa == null)
            {
                return NotFound(new
                {
                    status = 0,
                    message = "Không tìm thấy sản phẩm này"
                });
            }

            if (cartItem.Quantity <= 1)
            {
                return Ok(new
                {
                    status = 0,
                    message = "Số lượng phải >= 1",
                    qtt = cartItem.Quantity
                });
            }
            else
            {
                cartItem.Quantity--;
                _cart.SaveCart(cartList);
                return Ok(new
                {
                    status = 1,
                    message = "Đã giảm số lượng",
                    qtt = cartItem.Quantity
                });
            }
        }

        [HttpDelete]
        [Route("/cart/delete-item")]
        public async Task<IActionResult> DeleteCartItemAsync(string Id)
        {
            var cartList = _cart.GetCart();
            var cartItem = cartList?.FirstOrDefault(c => c.Id == Id);
            var capa = await _context.Capacities.FindAsync(cartItem?.Capacity.Id);

            if (cartItem == null || capa == null)
                return Json(new
                {
                    status = 0,
                    message = "Lỗi. Không tìm thấy sản phẩm này trong giỏ hàng!"
                });
            else
                cartList?.Remove(cartItem);

            _cart.SaveCart(cartList);

            return Ok(new
            {
                status = 1,
                message = "Đã xóa sản phẩm!",
                qtt = cartList?.Count
            });
        }

        [HttpGet]
        [Route("/cart/get-cart")]
        public IActionResult GetCartList()
        {
            var cartList = _cart.GetCart();
            return Ok(JsonConvert.SerializeObject(cartList));
        }

        [HttpGet]
        [Route("/order")]
        public IActionResult CreateOrder(string[] cartId)
        {
            var cartList = _cart.GetCart();
            var cartItemSelected = cartList?.Where(c => cartId.Contains(c.Id)).ToList();


            ViewBag.CartList = cartItemSelected;
            return View();
        }

        [HttpPost, ActionName(nameof(CreateOrder))]
        [ValidateAntiForgeryToken]
        [Route("/order")]
        public async Task<IActionResult> CreateOrderAsync(Order model, string[] cartId)
        {
            var cartList = _cart.GetCart();
            var cartItemSelected = cartList?.Where(c => cartId.Contains(c.Id)).ToList();
            ViewBag.CartList = cartItemSelected;

            if (cartItemSelected == null || cartItemSelected.Count == 0)
            {
                StatusMessage = "Có lỗi xảy ra, không tìm thấy sản phẩm";
                return View(model);
            }


            var orderDetailList = new List<OrderDetail>();

            cartItemSelected?.ForEach(item =>
            {
                var percentDiscount = item.Product.ProductDiscounts.Sum(x => x.Discount.PercentDiscount);
                var moneyDiscount = item.Product.ProductDiscounts.Sum(x => x.Discount.MoneyDiscount);

                orderDetailList.Add(new OrderDetail()
                {
                    ProductId = item.Product.Id,
                    ColorId = item.Color.Id,
                    CapacityId = item.Capacity.Id,
                    Quantity = item.Quantity,
                    SellPrice = item.Capacity.SellPrice * (100 - percentDiscount) / 100 - moneyDiscount
                });
            });

            var totalCost = orderDetailList.Sum(x => x.SellPrice);


            var now = DateTime.Now;

            var orderStatus = new OrderStatus()
            {
                Code = (int)OrderStatusCode.WaitAccept,
                Status = OrderStatuses.WaitAccept,
                DateUpdate = now,
                Note = "Khách hàng đặt hàng trên trang web",
            };

            var order = new Order()
            {
                FullName = model.FullName,
                City = model.City,
                Commune = model.Commune,
                District = model.District,
                Email = model.Email,
                PayType = model.PayType,
                PhoneNumber = model.PhoneNumber,
                OrderDate = now,
                SpecificAddress = model.SpecificAddress,
                Code = now.ToString("yyyyMMddHHmm") + model.PhoneNumber,
                TotalCost = totalCost,
                OrderDetails = orderDetailList,
                OrderStatuses = new List<OrderStatus>() {
                    orderStatus
                }
            };

            var user = await _userManager.GetUserAsync(User);

            if (user != null)
            {
                order.UserId = user.Id;
                orderStatus.UserId = user.Id;
            }


            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();

            _cart.SaveCartSub(cartItemSelected);

            string orderMessage = user == null ? "Chúng tôi sẽ liên lạc với bạn để xác nhận đơn hàng này. Nếu không xác nhận được đơn hàng sẽ bị hủy." : "Đơn hàng của bạn đang được chúng tôi xử lý.";

            string emailContent =
$@"
Cảm ơn bạn đã đặt hàng. {orderMessage}
Vui lòng theo dõi đơn hàng trong mục Theo dõi đơn hàng hoặc <a href='{Url.Action("OrderCheck", "ViewProduct", new { area = "Products", PhoneNumber = order.PhoneNumber, Code = order.Code }, HttpContext.Request.Scheme, HttpContext.Request.Host.Value)}'>nhấn vào đây</a>.

Mã đơn hàng: {order.Code}

Mọi thông tin chi tiết vui lòng liên hệ 1800.1789
Xin cảm ơn.";

            string emailHtml = AppUtilities.GenerateHtmlEmail(order.FullName, emailContent);

            _ = _emailSender.SendEmailAsync(order.Email, "Đặt hàng thành công", emailHtml);

            if (model.PayType == "Online")
            {
                string redirectUrl = _vnPay.SendRequest((long)totalCost, order.Code);
                return Redirect(redirectUrl);
            }
            else
            {
                return RedirectToAction(nameof(OrderConfirmed), new { orderId = order.Id });
            }

        }

        [Route("/thanh-toan/{orderId:int}")]
        public async Task<IActionResult> Pay(int orderId)
        {
            var order = await _context.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null) return NotFound();

            string redirectUrl = _vnPay.SendRequest((long)order.TotalCost, order.Code);
            return Redirect(redirectUrl);
        }

        [Route("/ket-qua-thanh-toan")]
        public async Task<IActionResult> PaymentResult([FromQuery] PayResponse model)
        {
            MatchCollection matches = Regex.Matches(model.vnp_OrderInfo ?? "", @"\d{22}");
            string orderId = "";
            if (matches.Count > 0)
            {
                foreach (Match item in matches)
                {
                    orderId = item.Value;
                }
            }
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Code == orderId);
            if (order == null) return NotFound();
            ViewBag.Order = order;

            var payStatusCheck = _context.PayStatuses.FirstOrDefault(p => p.PaymentCode == orderId);
            if (payStatusCheck != null)
                return View(payStatusCheck);

            var payStatus = new PayStatus()
            {
                Amount = Convert.ToDecimal(model.vnp_Amount),
                BankCode = model.vnp_BankCode,
                BankTranNo = model.vnp_BankTranNo,
                CardType = model.vnp_CardType,
                Date = DateTime.ParseExact(model.vnp_PayDate, "yyyyMMddHHmmss", null),
                OrderId = order.Id,
                OrderInfo = model.vnp_OrderInfo,
                PaymentCode = model.vnp_TxnRef,
                ResponseCode = model.vnp_ResponseCode,
                TransactionNo = model.vnp_TransactionNo,
                TransactionStatus = model.vnp_TransactionStatus,
            };

            switch (model.vnp_ResponseCode)
            {
                case "00":
                    payStatus.Content = "Giao dịch thành công";
                    string emailContent =
$@"
Bạn đã thanh toán đơn hàng {order.Code}. Chúng tôi đang chuẩn bị đơn hàng giao cho đơn vị vận chuyển.
Vui lòng theo dõi đơn hàng trong mục Theo dõi đơn hàng hoặc <a href='{Url.Action("OrderCheck", "ViewProduct", new { area = "Products", PhoneNumber = order.PhoneNumber, Code = order.Code }, HttpContext.Request.Scheme, HttpContext.Request.Host.Value)}'>nhấn vào đây</a>.

Mã đơn hàng: {order.Code}
Mã giao dịch: {payStatus.BankTranNo}
Thời gian: {(payStatus.Date ?? DateTime.Now).ToString("HH:mm dd/MM/yyyy")}
Số tiền: {(payStatus.Amount / 100 ?? 0).ToString("N0", new CultureInfo("vi-VN"))}đ

Mọi thông tin chi tiết vui lòng liên hệ 1800.1789
Xin cảm ơn.";

                    string emailHtml = AppUtilities.GenerateHtmlEmail(order.FullName, emailContent);

                    _ = _emailSender.SendEmailAsync(order.Email, "Thanh toán thành công", emailHtml);

                    order.OrderStatuses.Add(new OrderStatus()
                    {
                        Code = (int)OrderStatusCode.Accepted,
                        DateUpdate = DateTime.Now,
                        Status = OrderStatuses.Accepted,
                        Note = $"Đơn hàng đã thanh toán và xác nhận",

                    });

                    var cartList = _cart.GetCart();
                    var cartListToRemove = _cart.GetCartSub()?.Select(x => x.Id).ToList();
                    if (cartList != null && cartListToRemove != null)
                    {
                        foreach (var item in cartListToRemove)
                        {
                            var cartItemToRemove = cartList?.FirstOrDefault(x => x.Id == item);
                            if (cartItemToRemove != null)
                                cartList?.Remove(cartItemToRemove);
                        }
                    }
                    _cart.SaveCart(cartList);

                    break;
                case "07":
                    payStatus.Content = "Trừ tiền thành công. Giao dịch bị nghi ngờ (liên quan tới lừa đảo, giao dịch bất thường).";
                    break;
                case "09":
                    payStatus.Content = "Giao dịch không thành công do: Thẻ/Tài khoản của khách hàng chưa đăng ký dịch vụ InternetBanking tại ngân hàng.";
                    break;
                case "10":
                    payStatus.Content = "Giao dịch không thành công do: Khách hàng xác thực thông tin thẻ/tài khoản không đúng quá 3 lần.";
                    break;
                case "11":
                    payStatus.Content = "Giao dịch không thành công do: Đã hết hạn chờ thanh toán. Xin quý khách vui lòng thực hiện lại giao dịch.";
                    break;
                case "12":
                    payStatus.Content = "Giao dịch không thành công do: Thẻ/Tài khoản của khách hàng bị khóa.";
                    break;
                case "13":
                    payStatus.Content = "Giao dịch không thành công do Quý khách nhập sai mật khẩu xác thực giao dịch (OTP). Xin quý khách vui lòng thực hiện lại giao dịch.";
                    break;
                case "24":
                    payStatus.Content = "Giao dịch không thành công do: Khách hàng hủy giao dịch";
                    break;
                case "51":
                    payStatus.Content = "Giao dịch không thành công do: Tài khoản của quý khách không đủ số dư để thực hiện giao dịch.";
                    break;
                case "65":
                    payStatus.Content = "Giao dịch không thành công do: Tài khoản của Quý khách đã vượt quá hạn mức giao dịch trong ngày.";
                    break;
                case "75":
                    payStatus.Content = "Ngân hàng thanh toán đang bảo trì.";
                    break;
                case "79":
                    payStatus.Content = "Giao dịch không thành công do: KH nhập sai mật khẩu thanh toán quá số lần quy định. Xin quý khách vui lòng thực hiện lại giao dịch";
                    break;
                case "99":
                    payStatus.Content = "Các lỗi khác.";
                    break;
            }

            await _context.PayStatuses.AddAsync(payStatus);
            await _context.SaveChangesAsync();

            return View(payStatus);
        }

        [Route("/order-confirmed/{orderId:int}")]
        public async Task<IActionResult> OrderConfirmed(int orderId)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null) return NotFound();

            var cartList = _cart.GetCart();
            var cartListToRemove = _cart.GetCartSub()?.Select(x => x.Id).ToList();
            if (cartList != null && cartListToRemove != null)
            {
                foreach (var item in cartListToRemove)
                {
                    var cartItemToRemove = cartList?.FirstOrDefault(x => x.Id == item);
                    if (cartItemToRemove != null)
                        cartList?.Remove(cartItemToRemove);
                }
            }
            _cart.SaveCart(cartList);

            return View(order);
        }


        [Route("/order-check")]
        public async Task<IActionResult> OrderCheck(string? PhoneNumber, string? Code)
        {
            if (PhoneNumber != null && Code != null)
            {
                var order = await _context.Orders.Where(o => o.PhoneNumber == PhoneNumber && o.Code == Code)
                                            .Include(o => o.OrderDetails)
                                            .Include(o => o.OrderStatuses)
                                            .Include(o => o.PayStatuses)
                                            .AsSplitQuery()
                                            .FirstOrDefaultAsync();

                if (order == null)
                {
                    ViewBag.Message = "//Không tìm thấy đơn hàng";
                    return View();
                }

                order?.OrderDetails.ForEach(o =>
                {
                    o.Product = _context.Products.FirstOrDefault(p => p.Id == o.ProductId);
                    o.Color = _context.Colors.FirstOrDefault(c => c.Id == o.ColorId);
                    o.Capacity = _context.Capacities.FirstOrDefault(c => c.Id == o.CapacityId);
                });

                if (order?.OrderStatuses != null)
                {
                    order.OrderStatuses = order.OrderStatuses.OrderBy(s => s.DateUpdate).ToList();
                }
                return View(order);
            }
            else
            {
                return View();
            }
        }
    }
}