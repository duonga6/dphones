using App.Areas.Products.Models;
using App.Models;
using App.Models.Products;
using Microsoft.EntityFrameworkCore;
namespace App.Services
{
    public class HomeCategoryService
    {
        public readonly AppDbContext _context;

        public HomeCategoryService(AppDbContext context)
        {
            _context = context;
        }

        public HomeCategory GetData()
        {
            var productBestSell = _context.Products
                                            .Include(p => p.Colors.OrderBy(c => c.Name))
                                            .ThenInclude(c => c.Capacities)
                                            .AsSplitQuery()
                                            .Select(p => new {
                                                product = p,
                                                TotalSold = p.Colors
                                                            .SelectMany(c => c.Capacities)
                                                            .Sum(c => c.Sold)
                                            })
                                            .OrderByDescending(p => p.TotalSold)
                                            .Take(5)
                                            .Select(p => new ProductWithRate {
                                                Product = p.product,
                                                Rate = p.product.Reviews.Count == 0 ? 0 : p.product.Reviews.Average(r => r.Rate)
                                            })
                                            .ToList();


            HomeCategory data = new()
            {
                Brands = _context.Brands.OrderBy(b => b.Name).ToList(),
                Categories = _context.Categories.OrderBy(c => c.Name).ToList(),
                Products = productBestSell,
                PriceLevels = _context.PriceLevels.OrderBy(p => p.Level).Select(p => p.Level).ToList()
            };

            return data;
        }
    }
}