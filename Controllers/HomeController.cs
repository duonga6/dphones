using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using App.Models;
using Microsoft.EntityFrameworkCore;

namespace App.Controllers;
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly AppDbContext _context;

    public HomeController(ILogger<HomeController> logger, AppDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var now = DateTime.Now;

        var productQuery = _context.Products.Where(x => x.Published)
                            .AsNoTracking()
                            .Include(p => p.Colors.OrderBy(x => x.Name))
                                .ThenInclude(c => c.Capacities.OrderBy(x => x.SellPrice))
                            .Include(p => p.Reviews)
                            .Include(p => p.ProductDiscounts)
                                .ThenInclude(p => p.Discount)
                            .OrderByDescending(p => p.Colors.SelectMany(c => c.Capacities).Sum(c => c.Sold))
                            .AsSingleQuery();

        // SP giảm giá
        ViewBag.Discount = await productQuery
                            .Where(p => p.ProductDiscounts.Any(c => c.Discount.StartAt <= now && c.Discount.EndAt >= now))
                            .ToListAsync();

        ViewBag.SamsungHot = await productQuery
                            .Where(p => p.Brand!.Name == "Samsung")
                            .Take(10)
                            .ToListAsync();

        ViewBag.IPhoneHot = await productQuery
                            .Where(p => p.Brand!.Name == "Apple")
                            .Take(10)
                            .ToListAsync();

        ViewBag.XiaomiHot = await productQuery
                            .Where(p => p.Brand!.Name == "Xiaomi")
                            .Take(10)
                            .ToListAsync();

        ViewBag.RealmeHot = await productQuery
                            .Where(p => p.Brand!.Name == "Realme")
                            .Take(10)
                            .ToListAsync();

        // Post
        var posts = await _context.Posts.AsNoTracking().OrderByDescending(x => x.CreatedAt).Take(4).ToListAsync();
        ViewBag.Posts = posts;

        return View();
    }


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
