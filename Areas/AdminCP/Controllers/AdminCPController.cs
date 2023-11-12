using App.Data;
using App.Models;
using App.Models.Products;
using Bogus.DataSets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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

        public class ProductWithSold {
            public int Id {set;get;}
            public string Name {set;get;} = null!;
            public int Sold {set;get;}
            public string Image {set;get;} = null!;
        }

        public AdminCPController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index([FromQuery] string? revenure)
        {
            // Ngày/tháng/năm hôm nay
            var today = DateTime.Today;
            // Ngày hiện tại
            var dayNow = today.Day;
            // Tháng hiện tại
            var monthNow = today.Month;
            // Năm hiện tại
            var yearNow = today.Year;


            var orderQuery = _context.Orders;
            var soldQuery = _context.OrderDetails
            .Where(o => o.Order!.OrderStatuses.OrderBy(os => os.DateUpdate).Last().Code == (int)OrderStatusCode.Delivered);


            ViewBag.ProductCount = _context.Products.Count();
            ViewBag.BrandCount = _context.Brands.Count();
            ViewBag.OrderCount = orderQuery.Count();
            ViewBag.CustomerCount = (await _userManager.GetUsersInRoleAsync("Customer")).Count;
            ViewBag.ProductTotal = _context.Capacities.Sum(c => c.Quantity);

            ViewBag.TotalRevenue = soldQuery.Sum(o => (o.Capacity!.SellPrice - o.Capacity.EntryPrice) * o.Quantity);
            ViewBag.TotalSold = soldQuery.Sum(o => o.Quantity);

            ViewBag.OrderNotPayed = orderQuery.Where(o => !o.PayStatuses.Any(ps => ps.ResponseCode == "00") && !o.OrderStatuses.Any(od => od.Code == (int)OrderStatusCode.Canceled)).Count();
            ViewBag.OrderWaiting = orderQuery.Where(o => o.OrderStatuses.OrderBy(os => os.DateUpdate).Last().Code == (int)OrderStatusCode.WaitAccept).Count();
            ViewBag.OrderNotDelivering = orderQuery.Where(o => o.OrderStatuses.OrderBy(os => os.DateUpdate).Last().Code == (int)OrderStatusCode.Accepted).Count();
            ViewBag.OrderDelivering = orderQuery.Where(o => o.OrderStatuses.OrderBy(os => os.DateUpdate).Last().Code == (int)OrderStatusCode.Delivering).Count();
            ViewBag.OrderDelivered = orderQuery.Where(o => o.OrderStatuses.OrderBy(os => os.DateUpdate).Last().Code == (int)OrderStatusCode.Delivered).Count();
            ViewBag.OrderCanceled = orderQuery.Where(o => o.OrderStatuses.OrderBy(os => os.DateUpdate).Last().Code == (int)OrderStatusCode.Canceled).Count();


            // Thống kê doanh thu
            Dictionary<string, string> dataRevenure = new();
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
                                .Sum(od => (od.Capacity!.SellPrice - od.Capacity.EntryPrice) * od.Quantity)
                                .ToString("0");
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
                            .Sum(od => (od.Capacity!.SellPrice - od.Capacity.EntryPrice) * od.Quantity)
                            .ToString("0");
                            dataRevenure.Add($"{i}-{(i + stepDay > dayOfMonth ? dayOfMonth : i + stepDay)}/{monthNow}", result);
                        }
                    }
                    break;
                case "thisyear":
                    for (int i = 1; i <= 11; i++)
                    {
                        var result = orderQuery
                        .Where(o => o.PayStatuses.Any(ps => ps.ResponseCode == "00" && ps.Date!.Value.Year == yearNow && ps.Date.Value.Month == i))
                        .SelectMany(o => o.OrderDetails)
                        .Sum(od => (od.Capacity!.SellPrice - od.Capacity.EntryPrice) * od.Quantity)
                        .ToString("0");
                        dataRevenure.Add($"Tháng {i} - {i + 1}", result);
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
                            .Sum(od => (od.Capacity!.SellPrice - od.Capacity.EntryPrice) * od.Quantity)
                            .ToString("0");
                            dataRevenure.Add($"{i}-{i + stepDay}h", result);
                        }
                    }
                    break;
            }
            ViewBag.DataRevenure = dataRevenure;

            // Sản phẩm bán chạy tháng
            var Month = DateTime.Now.Month;
            var Year = DateTime.Now.Year;
            
            var productBSM = _context.OrderDetails
            .Where(od => od.Order!.OrderDate.Month == Month && od.Order.OrderDate.Year ==  Year && od.Order.OrderStatuses.Any(os => os.Code == (int)OrderStatusCode.Delivered))
            .Include(p => p.Color)
            .GroupBy(od => od.Product)
            .Select(od => new ProductWithSold{
                Id = od.Key!.Id,
                Image = od.FirstOrDefault()!.Color!.Image!,
                Name = od.Key!.Name,
                Sold = od.Sum(c => c.Quantity)
            })
            .OrderByDescending(p => p.Sold)
            .Take(5)
            .ToList();

            // return Ok(productBSM);

            ViewBag.ProductBSM = productBSM;

            // Doanh thu hôm nay
            var revenureToday = soldQuery.Where(od => od.Order!.OrderDate.Date == today.Date).Sum(od => (od.Capacity!.SellPrice - od.Capacity.EntryPrice) * od.Quantity);
            ViewBag.RevenureToday = revenureToday;

            return View();
        }
    }
}