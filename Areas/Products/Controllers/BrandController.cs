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
    [Route("/BrandManager/[action]")]
    [Authorize(Roles = RoleName.Administrator)]
    public class BrandController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<BrandController> _logger;

        [TempData]
        public string? StatusMessage { set; get; }

        private readonly int ITEM_PER_PAGE = 10;

        public BrandController(ILogger<BrandController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index([FromQuery(Name = "p")] int currentPage, [FromQuery(Name = "s")] string? searchString)
        {
            var brands = _context.Brands.Include(b => b.Products).AsNoTracking().AsQueryable();

            if (searchString != null)
            {
                brands = brands.Where(b => b.Name.Contains(searchString) || (b.Description != null && b.Description.Contains(searchString)));
            }

            brands = brands.OrderBy(b => b.Name);

            int countPage = (int)Math.Ceiling((decimal)brands.Count() / ITEM_PER_PAGE);
            if (countPage < 1)
                countPage = 1;

            if (currentPage < 1)
                currentPage = 1;
            if (currentPage > countPage)
                currentPage = countPage;

            ViewBag.CurrentPage = currentPage;
            ViewBag.CountPage = countPage;

            var brandInPage = brands.Skip((currentPage - 1) * ITEM_PER_PAGE).Take(ITEM_PER_PAGE);

            return View(await brandInPage.ToListAsync());
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost, ActionName(nameof(Create))]
        public async Task<IActionResult> CreateAsync(Brand model)
        {
            if (!ModelState.IsValid) return View(model);

            if (await _context.Brands.AsNoTracking().AnyAsync(b => b.Name == model.Name))
            {
                ModelState.AddModelError(string.Empty, "Hãng này đã tồn tại");
                return View();
            }

            model.Slug ??= AppUtilities.GenerateSlug(model.Name);

            await _context.Brands.AddAsync(model);
            await _context.SaveChangesAsync();

            StatusMessage = "Thêm hãng thành công";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var brand = await _context.Brands.Where(b => b.Id == id).FirstOrDefaultAsync();
            if (brand == null) return NotFound("Không tìm thấy hãng này");

            return View(brand);
        }

        [HttpPost("{id}"), ActionName(nameof(Edit))]
        public async Task<IActionResult> EditAsync(int id, Brand model)
        {
            var brand = await _context.Brands.Where(b => b.Id == id).FirstOrDefaultAsync();
            if (brand == null) return NotFound("Không tìm thấy hãng này");

            if (await _context.Brands.AnyAsync(b => b.Name == model.Name && b.Id != id))
            {
                ModelState.AddModelError(string.Empty, "Đã có hãng này");
                return View(model);
            }

            brand.Name = model.Name;
            brand.Description = model.Description;
            brand.Slug = string.IsNullOrEmpty(model.Slug) ? AppUtilities.GenerateSlug(model.Name) : model.Slug;

            await _context.SaveChangesAsync();
            StatusMessage = "Cập nhật thành công";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var brand = await _context.Brands.Where(b => b.Id == id).FirstOrDefaultAsync();
            if (brand == null) return NotFound("Không tìm thấy hãng này");

            ViewBag.Brand = brand;

            return View();
        }

        [HttpPost("{id}"), ActionName(nameof(Delete))]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var brand = await _context.Brands.Where(b => b.Id == id).FirstOrDefaultAsync();
            if (brand == null) return NotFound("Không tìm thấy hãng này");

            _context.Brands.Remove(brand);
            await _context.SaveChangesAsync();

            StatusMessage = "Xóa thành công";

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var brand = await _context.Brands.AsNoTracking().Where(b => b.Id == id).FirstOrDefaultAsync();
            if (brand == null) return NotFound();


            return View(brand);
        }
    }
}