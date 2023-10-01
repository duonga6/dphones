using App.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace App.Areas.Products.Controllers
{
    [Area("Products")]
    public class ViewProductController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ProductController> _logger;

        public ViewProductController(ILogger<ProductController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [Route("/{slug}")]
        public IActionResult Details(string slug)
        {
            var product = _context.Products.Where(p => p.Slug == slug).Include(p => p.Photos).FirstOrDefault();
            if (product == null)    return NotFound();


            return View(product);
        }
    }
}