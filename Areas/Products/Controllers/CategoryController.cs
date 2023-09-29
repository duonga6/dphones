using App.Data;
using App.Models;
using App.Models.Products;
using App.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Areas.Products.Controllers
{
    [Area("Products")]
    [Route("/CategoryManager/[action]")]
    [Authorize(Roles = RoleName.Administrator)]
    public class CategoryController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CategoryController> _logger;

        [TempData]
        public string? StatusMessage { set; get; }

        private readonly int ITEM_PER_PAGE = 10;

        public CategoryController(ILogger<CategoryController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index([FromQuery(Name = "p")] int currentPage, [FromQuery(Name = "s")] string? searchString)
        {
            var category = _context.Categories.Select(c => c);

            if (searchString != null)
            {
                searchString = searchString.ToLower();
                category = category.Where(c => c.Name.ToLower().Contains(searchString) || (c.Description != null && c.Description.ToLower().Contains(searchString)));
            }

            category = category.OrderBy(c => c.Name);

            int totalPage = (int)Math.Ceiling((double)category.Count() / ITEM_PER_PAGE);

            if (totalPage < 1)
                totalPage = 1;

            if (currentPage < 1)
                currentPage = 1;
            
            if (currentPage > totalPage)
                currentPage = totalPage;
            
            ViewBag.CurrentPage = currentPage;
            ViewBag.CountPage = totalPage;

            var categoryInPage = category.Skip((currentPage - 1) * ITEM_PER_PAGE).Take(ITEM_PER_PAGE);

            return View(categoryInPage.ToList());
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost, ActionName(nameof(Create))]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAsync(Category model)
        {
            if (!ModelState.IsValid) return View();

            model.Slug ??= AppUtilities.GenerateSlug(model.Name);

            if (_context.Categories.Where(c => c.Name == model.Name).Any())
            {
                ModelState.AddModelError(string.Empty, "Tên này đã được dùng");
                return View();
            }

            if (_context.Categories.Where(c => c.Slug == model.Slug).Any())
            {
                ModelState.AddModelError(string.Empty, "Địa chỉ url này đã được dùng");
                return View();
            }

            _context.Categories.Add(model);
            await _context.SaveChangesAsync();


            StatusMessage = "Thêm thành công";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("{Id}")]
        public IActionResult Edit(int Id)
        {
            var category = _context.Categories.Where(c => c.Id == Id).FirstOrDefault();
            if (category == null) return NotFound();

            return View(category);
        }

        [HttpPost("{Id}"), ActionName(nameof(Edit))]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAsync(int Id, Category model)
        {
            var category = _context.Categories.Where(c => c.Id == Id).FirstOrDefault();
            if (category == null) return NotFound();

            if(!ModelState.IsValid) return View(model);

            if (_context.Categories.Where(c => c.Name == model.Name && c.Id != Id).Any())
            {
                ModelState.AddModelError(string.Empty, "Tên này đã được dùng");
                return View(model);
            }

            if (_context.Categories.Where(c => c.Slug == model.Slug && c.Id != Id).Any())
            {
                ModelState.AddModelError(string.Empty, "Địa chỉ url này đã được dùng");
                return View(model);
            }

            category.Name = model.Name;
            category.Slug = model.Slug;
            category.Description = model.Description;
            
            await _context.SaveChangesAsync();

            StatusMessage = "Cập nhật thành công";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("{Id}")]
        public IActionResult Delete(int Id)
        {
            var category = _context.Categories.Where(c => c.Id == Id).FirstOrDefault();
            if (category == null) return NotFound();

            return View(category);
        }

        [HttpPost("{Id}"), ActionName(nameof(Delete))]
        public async Task<IActionResult> DeleteAsync(int Id)
        {
            var category = _context.Categories.Where(c => c.Id == Id).FirstOrDefault();
            if (category == null) return NotFound();

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            StatusMessage = "Xóa thành công";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Details(int id)
        {
            var category = _context.Categories.Where(c => c.Id == id).FirstOrDefault();
            if (category == null)
            return NotFound();
            
            return View(category);
        }
    }
}