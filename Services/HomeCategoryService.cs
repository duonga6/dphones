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
            var color = _context.Colors.Include(c => c.Capacities!)
                                    .Include(c => c.Product!)
                                    .GroupBy(c => c.ProductId)
                                    .AsEnumerable()
                                    .OrderByDescending(c => c.Sum(cap => cap.Capacities.Sum(ca => ca.Sold)))
                                    .Take(5)
                                    .ToList();

            var productsBestSeller = new List<Product>();
            color.ForEach(c =>
            {
                var productItem = c.FirstOrDefault()?.Product;
                if (productItem != null) 
                    productsBestSeller.Add(productItem);
            });

            HomeCategory data = new()
            {
                Brands = _context.Brands.OrderBy(b => b.Name).ToList(),
                Categories = _context.Categories.OrderBy(c => c.Name).ToList(),
                Products = productsBestSeller
            };

            return data;
        }
    }
}