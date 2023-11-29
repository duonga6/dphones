using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using App.Models;
using Microsoft.EntityFrameworkCore;
using App.Areas.Products.Models;

namespace App.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly AppDbContext _context;

    private readonly int ITEM_PER_PAGE = 20;

    public HomeController(ILogger<HomeController> logger, AppDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index([FromQuery(Name = "p")] int currentPage)
    {

        int countPage = (int)Math.Ceiling((double)_context.Products.Count() / ITEM_PER_PAGE);

        if (countPage < 1)
            countPage = 1;

        if (currentPage < 1)
            currentPage = 1;

        if (currentPage > countPage)
            currentPage = countPage;

        ViewBag.CurrentPage = currentPage;
        ViewBag.CountPage = countPage;

        ViewBag.Discount = _context.Products
                            .Where(p => p.ProductDiscounts.Count > 0)
                            .Include(p => p.Colors)
                            .ThenInclude(c => c.Capacities)
                            .Include(p => p.Reviews)
                            .Include(p => p.ProductDiscounts.Where(c => c.Discount.StartAt <= DateTime.Now && c.Discount.EndAt >= DateTime.Now))
                            .ThenInclude(p => p.Discount)
                            .AsSplitQuery()
                            .Select(p => new
                            {
                                product = p,
                                totalSold = p.Colors.SelectMany(c => c.Capacities).Sum(c => c.Sold)
                            })
                            .OrderByDescending(p => p.totalSold)
                            .AsQueryable()
                            .Take(10)
                            .Select(p => p.product)
                            .ToList();

        ViewBag.SamsungHot = _context.Products
                            .Where(p => p.Brand!.Name == "Samsung")
                            .Include(p => p.Colors)
                            .ThenInclude(c => c.Capacities)
                            .Include(p => p.Reviews)
                            .AsSplitQuery()
                            .Select(p => new
                            {
                                product = p,
                                totalSold = p.Colors.SelectMany(c => c.Capacities).Sum(c => c.Sold)
                            })
                            .OrderByDescending(p => p.totalSold)
                            .AsQueryable()
                            .Take(10)
                            .Select(p => p.product)
                            .ToList();

        ViewBag.IPhoneHot = _context.Products
                            .Where(p => p.Brand!.Name == "Apple")
                            .Include(p => p.Colors)
                            .ThenInclude(c => c.Capacities)
                            .Include(p => p.Reviews)
                            .AsSplitQuery()
                            .Select(p => new
                            {
                                product = p,
                                totalSold = p.Colors.SelectMany(c => c.Capacities).Sum(c => c.Sold)
                            })
                            .OrderByDescending(p => p.totalSold)
                            .AsQueryable()
                            .Take(10)
                            .Select(p => p.product)
                            .ToList();

        ViewBag.XiaomiHot = _context.Products
                            .Where(p => p.Brand!.Name == "Xiaomi")
                            .Include(p => p.Colors)
                            .ThenInclude(c => c.Capacities)
                            .Include(p => p.Reviews)
                            .AsSplitQuery()
                            .Select(p => new
                            {
                                product = p,
                                totalSold = p.Colors.SelectMany(c => c.Capacities).Sum(c => c.Sold)
                            })
                            .OrderByDescending(p => p.totalSold)
                            .AsQueryable()
                            .Take(10)
                            .Select(p => p.product)
                            .ToList();

        ViewBag.RealmeHot = _context.Products
                            .Where(p => p.Brand!.Name == "Realme")
                            .Include(p => p.Colors)
                            .ThenInclude(c => c.Capacities)
                            .Include(p => p.Reviews)
                            .AsSplitQuery()
                            .Select(p => new
                            {
                                product = p,
                                totalSold = p.Colors.SelectMany(c => c.Capacities).Sum(c => c.Sold)
                            })
                            .OrderByDescending(p => p.totalSold)
                            .AsQueryable()
                            .Take(10)
                            .Select(p => p.product)
                            .ToList();
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
