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
            public required Product Product {set;get;}
            public int Sold {set;get;}
        }

        public AdminCPController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index([FromQuery] string? revenure)
        {
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
                        var today = DateTime.Today;
                        int thisDay = (int)today.DayOfWeek;
                        var firstDayOfWeek = today.AddDays(-thisDay);
                        for (int i = 1; i <= 7; i++)
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
                        int thisMonth = DateTime.Now.Month;
                        int thisDay = DateTime.Now.Day;
                        int dayOfMonth = DateTime.DaysInMonth(DateTime.Now.Year, thisMonth);
                        for (int i = 1; i < dayOfMonth; i += stepDay)
                        {
                            var dateFrom = DateTime.Now.AddDays(-thisDay + i);
                            var dateTo = new DateTime();
                            if (i + stepDay > dayOfMonth)
                                dateTo = DateTime.Now.AddDays(-thisDay + dayOfMonth);
                            else
                                dateTo = DateTime.Now.AddDays(-thisDay + i + stepDay);
                            var result = orderQuery
                            .Where(o => o.PayStatuses.Any(ps => ps.ResponseCode == "00" && ps.Date >= dateFrom && ps.Date < dateTo))
                            .SelectMany(o => o.OrderDetails)
                            .Sum(od => (od.Capacity!.SellPrice - od.Capacity.EntryPrice) * od.Quantity)
                            .ToString("0");
                            dataRevenure.Add($"{i}-{(i + stepDay > dayOfMonth ? dayOfMonth : i + stepDay)}/{thisMonth}", result);
                        }
                    }
                    break;
                case "thisyear":
                    var thisYear = DateTime.Now.Year;
                    for (int i = 1; i <= 11; i++)
                    {
                        var result = orderQuery
                        .Where(o => o.PayStatuses.Any(ps => ps.ResponseCode == "00" && ps.Date!.Value.Year == thisYear && ps.Date.Value.Month == i))
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

            // Sản phẩm bán chạy

            // var productsBestSell 


            return View();
        }
    }
}