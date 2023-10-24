using App.Data;
using App.Models;
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

            Dictionary<string, string> dataRevenure = new();
            switch (revenure)
            {
                case "thisweek":
                    break;
                case "thismonth":
                    break;
                case "thisyear":
                    break;
                default:
                    for (int i = 0; i <= 23; i += 2)
                    {
                        var dateFrom = DateTime.Today.AddDays(-1).AddHours(i);
                        var dateTo = DateTime.Today.AddDays(-1).AddHours(i + 2);
                        var result = orderQuery.Where(o => o.PayStatuses.Any(ps => ps.ResponseCode == "00" && ps.Date >= dateFrom && ps.Date < dateTo)).Sum(o => o.TotalCost).ToString("0");
                        dataRevenure.Add($"{i}h", result);
                    }
                    break;
            }

            ViewBag.DataRevenure = dataRevenure;


            return View();
        }
    }
}