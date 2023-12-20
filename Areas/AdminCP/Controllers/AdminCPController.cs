using App.Areas.AdminCP.Models;
using App.Data;
using App.Models;
using App.Models.Products;
using App.Utilities;
using Bogus.DataSets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace App.Areas.AdminCP.Controllers
{
    [Area("AdminCP")]
    [Route("/AdminCP/[action]")]
    [Authorize(Roles = RoleName.Administrator)]
    public class AdminCPController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailSender _emailSender;
        [TempData]
        public string StatusMessage { set; get; } = "";
        public class ProductWithSold
        {
            public int Id { set; get; }
            public string Name { set; get; } = null!;
            public int Sold { set; get; }
            public string Image { set; get; } = null!;
        }

        public AdminCPController(AppDbContext context, UserManager<AppUser> userManager, IEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        public async Task<IActionResult> Index([FromQuery] string? revenure, [FromQuery] string? profit)
        {
            // Ngày/tháng/năm hôm nay
            var today = DateTime.Today;
            // Ngày hiện tại
            var dayNow = today.Day;
            // Tháng hiện tại
            var monthNow = today.Month;
            // Năm hiện tại
            var yearNow = today.Year;


            var orderQuery = _context.Orders.AsNoTracking();
            var soldQuery = orderQuery
            .Where(o => o.PayStatuses.Any(x => x.ResponseCode == "00"));
            // .Where(o => o.Order!.OrderStatuses.OrderBy(os => os.DateUpdate).Last().Code == (int)OrderStatusCode.Delivered);


            // Thống kê tổng quát
            ViewBag.ProductCount = _context.Products.AsNoTracking().Count();
            ViewBag.BrandCount = _context.Brands.AsNoTracking().Count();
            ViewBag.OrderCount = orderQuery.AsNoTracking().Count();
            ViewBag.CustomerCount = (await _userManager.GetUsersInRoleAsync("Customer")).Count;
            ViewBag.ProductTotal = _context.Capacities.AsNoTracking().Sum(c => c.Quantity);

            // Tổng lợi nhuận
            ViewBag.TotalRevenue = soldQuery.SelectMany(o => o.OrderDetails)
                                .Sum(od => (od.SellPrice - od.Capacity!.EntryPrice) * od.Quantity);

            // Tổng doanh thu
            ViewBag.TotalProfit = soldQuery.SelectMany(o => o.OrderDetails)
                                .Sum(od => od.Capacity!.SellPrice * od.Quantity);

            // Tổng đã bán
            ViewBag.TotalSold = soldQuery.SelectMany(o => o.OrderDetails)
                                .Sum(od => od.Quantity);

            // Dánh sách trạng thái đơn hàng
            ViewBag.OrderNotPayed = orderQuery.Where(o => !o.PayStatuses.Any(ps => ps.ResponseCode == "00") && !o.OrderStatuses.Any(od => od.Code == (int)OrderStatusCode.Canceled)).Count();
            ViewBag.OrderWaiting = orderQuery.Where(o => o.OrderStatuses.OrderBy(os => os.DateUpdate).Last().Code == (int)OrderStatusCode.WaitAccept).Count();
            ViewBag.OrderNotDelivering = orderQuery.Where(o => o.OrderStatuses.OrderBy(os => os.DateUpdate).Last().Code == (int)OrderStatusCode.Accepted).Count();
            ViewBag.OrderDelivering = orderQuery.Where(o => o.OrderStatuses.OrderBy(os => os.DateUpdate).Last().Code == (int)OrderStatusCode.Delivering).Count();
            ViewBag.OrderDelivered = orderQuery.Where(o => o.OrderStatuses.OrderBy(os => os.DateUpdate).Last().Code == (int)OrderStatusCode.Delivered).Count();
            ViewBag.OrderCanceled = orderQuery.Where(o => o.OrderStatuses.OrderBy(os => os.DateUpdate).Last().Code == (int)OrderStatusCode.Canceled).Count();


            // Thống kê doanh thu
            Dictionary<string, decimal> dataRevenure = new();
            Dictionary<string, decimal> dataProfit = new();
            switch (revenure)
            {

                case "thisweek":
                    {
                        int thisDay = (int)today.DayOfWeek;
                        var firstDayOfWeek = today.AddDays(-thisDay);
                        for (int i = 0; i <= 6; i++)
                        {
                            var dateFrom = firstDayOfWeek.AddDays(i);
                            var dateTo = firstDayOfWeek.AddDays(i + 1);
                            var result = orderQuery
                                .Where(o => o.PayStatuses.Any(ps => ps.ResponseCode == "00" && ps.Date >= dateFrom && ps.Date < dateTo))
                                .SelectMany(o => o.OrderDetails)
                                .Sum(od => od.Capacity!.SellPrice * od.Quantity);
                            dataRevenure.Add($"{dateFrom.DayOfWeek}", result);

                        }
                    }
                    break;

                case "thismonth":
                    {
                        int stepDay = 4;
                        int dayOfMonth = DateTime.DaysInMonth(DateTime.Now.Year, monthNow);
                        for (int i = 1; i < dayOfMonth; i += stepDay)
                        {
                            var dateFrom = DateTime.Now.AddDays(-dayNow + i);
                            var dateTo = new DateTime();
                            if (i + stepDay > dayOfMonth)
                                dateTo = DateTime.Now.AddDays(-dayNow + dayOfMonth);
                            else
                                dateTo = DateTime.Now.AddDays(-dayNow + i + stepDay);

                            var result = orderQuery
                            .Where(o => o.PayStatuses.Any(ps => ps.ResponseCode == "00" && ps.Date >= dateFrom && ps.Date < dateTo))
                            .SelectMany(o => o.OrderDetails)
                            .Sum(od => od.Capacity!.SellPrice * od.Quantity);
                            dataRevenure.Add($"{i}-{(i + stepDay > dayOfMonth ? dayOfMonth : i + stepDay)}/{monthNow}", result);

                        }
                    }
                    break;
                case "thisyear":
                    for (int i = 1; i <= 12; i++)
                    {
                        var result = soldQuery
                        .Where(o => o.PayStatuses.Any(ps => ps.Date!.Value.Year == yearNow && ps.Date.Value.Month == i))
                        .SelectMany(o => o.OrderDetails)
                        .Sum(od => od.Capacity!.SellPrice * od.Quantity);

                        dataRevenure.Add($"Tháng {i}", result);

                    }
                    break;
                default:
                    {
                        int stepDay = 2;
                        for (int i = 0; i <= 23; i += 2)
                        {
                            var dateFrom = DateTime.Today.AddHours(i);
                            var dateTo = DateTime.Today.AddHours(i + stepDay);
                            var result = orderQuery
                            .Where(o => o.PayStatuses.Any(ps => ps.ResponseCode == "00" && ps.Date >= dateFrom && ps.Date < dateTo))
                            .SelectMany(o => o.OrderDetails)
                            .Sum(od => od.Capacity!.SellPrice * od.Quantity);
                            dataRevenure.Add($"{i}-{i + stepDay}h", result);

                        }
                    }
                    break;
            }

            switch (profit)
            {

                case "thisweek":
                    {
                        int thisDay = (int)today.DayOfWeek;
                        var firstDayOfWeek = today.AddDays(-thisDay);
                        for (int i = 0; i <= 6; i++)
                        {
                            var dateFrom = firstDayOfWeek.AddDays(i);
                            var dateTo = firstDayOfWeek.AddDays(i + 1);

                            var profitResult = orderQuery
                                .Where(o => o.PayStatuses.Any(ps => ps.ResponseCode == "00" && ps.Date >= dateFrom && ps.Date < dateTo))
                                .SelectMany(o => o.OrderDetails)
                                .Sum(od => (od.SellPrice - od.Capacity!.EntryPrice) * od.Quantity);

                            dataProfit.Add($"{dateFrom.DayOfWeek}", profitResult);
                        }
                    }
                    break;

                case "thismonth":
                    {
                        int stepDay = 4;
                        int dayOfMonth = DateTime.DaysInMonth(DateTime.Now.Year, monthNow);
                        for (int i = 1; i < dayOfMonth; i += stepDay)
                        {
                            var dateFrom = DateTime.Now.AddDays(-dayNow + i);
                            var dateTo = new DateTime();
                            if (i + stepDay > dayOfMonth)
                                dateTo = DateTime.Now.AddDays(-dayNow + dayOfMonth);
                            else
                                dateTo = DateTime.Now.AddDays(-dayNow + i + stepDay);

                            var resultProfit = orderQuery
                            .Where(o => o.PayStatuses.Any(ps => ps.ResponseCode == "00" && ps.Date >= dateFrom && ps.Date < dateTo))
                            .SelectMany(o => o.OrderDetails)
                            .Sum(od => (od.SellPrice - od.Capacity!.EntryPrice) * od.Quantity);
                            dataProfit.Add($"{i}-{(i + stepDay > dayOfMonth ? dayOfMonth : i + stepDay)}/{monthNow}", resultProfit);
                        }
                    }
                    break;
                case "thisyear":
                    for (int i = 1; i <= 12; i++)
                    {
                        var resultProfit = soldQuery
                        .Where(o => o.PayStatuses.Any(ps => ps.Date!.Value.Year == yearNow && ps.Date.Value.Month == i))
                        .SelectMany(o => o.OrderDetails)
                        .Sum(od => (od.SellPrice - od.Capacity!.EntryPrice) * od.Quantity);

                        dataProfit.Add($"Tháng {i}", resultProfit);
                    }
                    break;
                default:
                    {
                        int stepDay = 2;
                        for (int i = 0; i <= 23; i += 2)
                        {
                            var dateFrom = DateTime.Today.AddHours(i);
                            var dateTo = DateTime.Today.AddHours(i + stepDay);

                            var resultProfit = orderQuery
                            .Where(o => o.PayStatuses.Any(ps => ps.ResponseCode == "00" && ps.Date >= dateFrom && ps.Date < dateTo))
                            .SelectMany(o => o.OrderDetails)
                            .Sum(od => (od.SellPrice - od.Capacity!.EntryPrice) * od.Quantity);
                            dataProfit.Add($"{i}-{i + stepDay}h", resultProfit);
                        }
                    }
                    break;
            }

            ViewBag.DataRevenure = dataRevenure;
            ViewBag.DataProfit = dataProfit;

            var Month = DateTime.Now.Month;
            var Year = DateTime.Now.Year;

            // Sản phẩm bán chạy tháng
            var productBSM = _context.OrderDetails.AsNoTracking()
            .Where(od => od.Order!.OrderDate.Month == Month && od.Order.OrderDate.Year == Year && od.Order.OrderStatuses.Any(os => os.Code == (int)OrderStatusCode.Delivered))
            .Include(p => p.Color)
            .GroupBy(od => od.Product)
            .Select(od => new ProductWithSold
            {
                Id = od.Key!.Id,
                Image = od.FirstOrDefault()!.Color!.Image!,
                Name = od.Key!.Name,
                Sold = od.Sum(c => c.Quantity)
            })
            .OrderByDescending(p => p.Sold)
            .Take(5)
            .ToList();
            ViewBag.ProductBSM = productBSM;

            // Doanh thu hôm nay
            var revenureToday = soldQuery.Where(od => od.OrderDate.Date == today.Date).SelectMany(o => o.OrderDetails)
                                .Sum(od => (od.SellPrice - od.Capacity!.EntryPrice) * od.Quantity);
            ViewBag.RevenureToday = revenureToday;

            return View();
        }


        // Gửi Mail
        [HttpGet]
        public IActionResult SendMail()
        {
            return View();
        }

        [HttpPost, ActionName(nameof(SendMail))]
        public async Task<IActionResult> SendMailAsync(SendMailModel model)
        {
            try
            {
                var mailContent = AppUtilities.GenerateHtmlEmail("quý khách", model.Content);
                if (model.Type == "One")
                {
                    if (model.Receiver == null || !AppUtilities.IsValidEmail(model.Receiver))
                    {
                        ViewBag.Error = "Email không hợp lệ";
                        return View(model);
                    }

                    await _emailSender.SendEmailAsync(model.Receiver, model.Subject, mailContent);
                }
                else if (model.Type == "Many")
                {
                    if (model.Receiver == null)
                    {
                        ViewBag.Error = "Chưa nhập email";
                        return View(model);
                    }

                    var emailSplit = model.Receiver.Split(",").ToList();
                    emailSplit = emailSplit.Select(x => x.Trim()).ToList();
                    if (emailSplit.Any(x => !AppUtilities.IsValidEmail(x)))
                    {
                        ViewBag.Error = "Có một email chưa hợp lệ";
                        return View(model);
                    }

                    foreach (var item in emailSplit)
                    {
                        await _emailSender.SendEmailAsync(item, model.Subject, mailContent);
                    }
                }
                else if (model.Type == "All")
                {
                    var emailUserList = _context.Users.AsNoTracking().Where(x => x.Email != null).Select(x => x.Email).ToList() as List<string>;
                    foreach (var item in emailUserList)
                    {
                        await _emailSender.SendEmailAsync(item, model.Subject, mailContent);
                    }
                }

                StatusMessage = "Gửi thành công";
                return RedirectToAction(nameof(SendMail));
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Có lỗi trong quá trình gửi: " + ex.Message;
                return View(model);
            }

        }

        [Route("/send-mail-api")]
        [HttpPost]
        public async Task<IActionResult> SendMailApiAsync([FromBody] SendMailModel model)
        {
            var mailContent = AppUtilities.GenerateHtmlEmail("quý khách", model.Content);
            if (model.Receiver == null || !AppUtilities.IsValidEmail(model.Receiver))
            {
                return BadRequest(new
                {
                    StatusCode = 0,
                    Message = "Email không hợp lệ"
                });
            }

            await _emailSender.SendEmailAsync(model.Receiver, model.Subject, mailContent);

            return Ok(new
            {
                StatusCode = 0,
                Message = "Gủi mail thành công"
            });
        }
    }
}