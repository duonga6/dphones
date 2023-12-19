using App.Data;
using App.Models;
using App.Models.Products;
using App.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        public async Task<IActionResult> Index([FromQuery(Name = "p")] int currentPage, [FromQuery(Name = "s")] string? searchString)
        {
            var category = _context.Categories.AsQueryable();

            if (searchString != null)
            {
                category = category.Where(c => c.Name.Contains(searchString) || (c.Description != null && c.Description.Contains(searchString)));
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

            return View(await categoryInPage.ToListAsync());
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

            if (await _context.Categories.AsNoTracking().AnyAsync(c => c.Name == model.Name))
            {
                ModelState.AddModelError(string.Empty, "Tên này đã được dùng");
                return View();
            }

            if (await _context.Categories.AsNoTracking().AnyAsync(c => c.Slug == model.Slug))
            {
                ModelState.AddModelError(string.Empty, "Địa chỉ url này đã được dùng");
                return View();
            }

            await _context.Categories.AddAsync(model);
            await _context.SaveChangesAsync();


            StatusMessage = "Thêm thành công";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("{Id}")]
        public async Task<IActionResult> Edit(int Id)
        {
            var category = await _context.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == Id);
            if (category == null) return NotFound();

            return View(category);
        }

        [HttpPost("{Id}"), ActionName(nameof(Edit))]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAsync(int Id, Category model)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == Id);
            if (category == null) return NotFound();

            if (!ModelState.IsValid) return View(model);

            if (await _context.Categories.AsNoTracking().AnyAsync(c => c.Name == model.Name && c.Id != Id))
            {
                ModelState.AddModelError(string.Empty, "Tên này đã được dùng");
                return View(model);
            }

            model.Slug ??= AppUtilities.GenerateSlug(model.Name, false);

            if (await _context.Categories.AsNoTracking().AnyAsync(c => c.Slug == model.Slug && c.Id != Id))
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
        public async Task<IActionResult> Delete(int Id)
        {
            var category = await _context.Categories.FindAsync(Id);
            if (category == null) return NotFound();

            return View(category);
        }

        [HttpPost("{Id}"), ActionName(nameof(Delete))]
        public async Task<IActionResult> DeleteAsync(int Id)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == Id);
            if (category == null) return NotFound();

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            StatusMessage = "Xóa thành công";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return NotFound();

            return View(category);
        }
    }
}