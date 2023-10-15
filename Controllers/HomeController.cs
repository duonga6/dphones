using System.Net.Mime;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using App.Models;
using Microsoft.EntityFrameworkCore;
using App.Models.Products;

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

    public IActionResult Index([FromQuery(Name = "p")]int currentPage)
    {

        int countPage = (int)Math.Ceiling((double)_context.Products.Count() / ITEM_PER_PAGE);

        if (countPage < 1)
            countPage = 1;

        if (currentPage < 1)
            currentPage = 1;
        
        if (currentPage > countPage)
            currentPage = countPage;

            
        var products = _context.Products.Include(p => p.Brand)
                                        .Include(p => p.Photos)
                                        .Include(p => p.Colors)
                                        .ThenInclude(c => c.Capacities)
                                        .AsSingleQuery()
                                        .OrderByDescending(p => p.EntryDate)
                                        .Skip((currentPage - 1) * ITEM_PER_PAGE).Take(ITEM_PER_PAGE)
                                        .ToList();
        
        products.ForEach(p => {
            p.Colors = p.Colors.OrderBy(c => c.Name).ToList();
            p.Colors.ForEach(cl => {
                cl.Capacities = cl.Capacities.OrderBy(ca => ca.Rom).ToList();
            });
        });
        
        
        ViewBag.Products = products;
        ViewBag.CurrentPage = currentPage;
        ViewBag.CountPage = countPage;

        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
