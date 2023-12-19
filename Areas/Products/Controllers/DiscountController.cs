using App.Areas.Products.Models;
using App.Models;
using App.Models.Products;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace App.Areas.Products.Controllers
{
    [Area("Products")]
    [Route("/[area]/[controller]/[action]")]
    public class DiscountController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<DiscountController> _logger;
        [TempData]
        public string StatusMessage { set; get; } = "";

        public DiscountController(ILogger<DiscountController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var discounts = await _context.Discounts
            .Include(x => x.ProductDiscounts)
            .ThenInclude(x => x.Product)
            .AsSplitQuery()
            .AsNoTracking()
            .ToListAsync();

            return View(discounts);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var products = await _context.Products.OrderBy(x => x.Name).AsNoTracking().ToListAsync();
            ViewBag.Products = products;
            return View();
        }

        [HttpPost, ActionName(nameof(Create))]
        public async Task<IActionResult> CreateAsync(CreateDiscountModel model)
        {
            var products = await _context.Products.OrderBy(x => x.Name).AsNoTracking().ToListAsync();
            ViewBag.Products = products;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.PercenDiscount == 0 && model.MoneyDiscount == 0)
            {
                StatusMessage = "Dữ liệu giảm giá không hợp lệ";
                return View(model);
            }

            if (model.StartAt >= model.EndAt)
            {
                StatusMessage = "Thời gian không hợp lệ";
                return View(model);
            }

            var newDiscount = new Discount()
            {
                Content = model.Content,
                StartAt = model.StartAt,
                EndAt = model.EndAt,
                PercentDiscount = model.PercenDiscount,
                MoneyDiscount = model.MoneyDiscount
            };

            await _context.Discounts.AddAsync(newDiscount);

            if (model.ProductIds != null)
            {
                foreach (var item in model.ProductIds)
                {
                    if (await _context.Products.FindAsync(item) == null)
                    {
                        StatusMessage = "Lỗi không tìm thấy sản phẩm có Id: " + item;
                        return View();
                    }

                    await _context.ProductDiscounts.AddAsync(new ProductDiscount()
                    {
                        Discount = newDiscount,
                        ProductId = item
                    });
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [Route("{Id}")]
        public async Task<IActionResult> Details(int Id)
        {
            var discounts = await _context.Discounts
            .AsNoTracking()
            .Include(x => x.ProductDiscounts)
            .ThenInclude(x => x.Product)
            .ThenInclude(x => x.Colors.OrderBy(c => c.Name))
            .AsSplitQuery()
            .FirstOrDefaultAsync(x => x.Id == Id);

            if (discounts == null) return NotFound();

            return View(discounts);
        }

        [HttpGet("{Id}")]
        public async Task<IActionResult> Edit(int Id)
        {
            var discount = await _context.Discounts
                                    .AsNoTracking()
                                    .Include(x => x.ProductDiscounts)
                                    .ThenInclude(x => x.Product)
                                    .ThenInclude(x => x.Colors)
                                    .AsSplitQuery()
                                    .FirstOrDefaultAsync(x => x.Id == Id);

            if (discount == null) return NotFound();

            var products = await _context.Products.OrderBy(x => x.Name).AsNoTracking().ToListAsync();
            ViewBag.Products = products;

            var discountModel = new CreateDiscountModel()
            {
                Content = discount.Content,
                EndAt = discount.EndAt,
                MoneyDiscount = discount.MoneyDiscount,
                PercenDiscount = discount.PercentDiscount,
                ProductIds = discount.ProductDiscounts.Select(x => x.ProductId).ToList(),
                StartAt = discount.StartAt,
            };

            return View(discountModel);
        }

        [HttpPost("{Id}"), ActionName(nameof(Edit))]
        public async Task<IActionResult> EditAsync(int Id, CreateDiscountModel model)
        {
            var updateDiscount = await _context.Discounts.Include(x => x.ProductDiscounts).FirstOrDefaultAsync(x => x.Id == Id);
            if (updateDiscount == null) return NotFound();

            var products = await _context.Products.OrderBy(x => x.Name).AsNoTracking().ToListAsync();
            ViewBag.Products = products;

            if (!ModelState.IsValid)
            {
                return View();
            }

            if (model.PercenDiscount == 0 && model.MoneyDiscount == 0)
            {
                StatusMessage = "Dữ liệu giảm giá không hợp lệ";
                return View();
            }

            if (model.StartAt >= model.EndAt)
            {
                StatusMessage = "Thời gian không hợp lệ";
                return View(model);
            }

            updateDiscount.Content = model.Content;
            updateDiscount.EndAt = model.EndAt;
            updateDiscount.StartAt = model.StartAt;
            updateDiscount.MoneyDiscount = model.MoneyDiscount;
            updateDiscount.PercentDiscount = model.PercenDiscount;

            var currentProductIds = updateDiscount.ProductDiscounts.Select(x => x.ProductId).ToList();
            var newProductIds = model.ProductIds ?? new List<int>();

            var addProductId = newProductIds.Where(x => !currentProductIds.Contains(x)).ToList();
            var removeProductId = currentProductIds.Where(x => !newProductIds.Contains(x)).ToList();

            var removeProductIdList = new List<ProductDiscount>();
            foreach (var item in removeProductId)
            {
                var rmItem = await _context.ProductDiscounts.FirstOrDefaultAsync(x => x.ProductId == item && x.DiscountId == updateDiscount.Id);
                if (rmItem == null)
                {
                    StatusMessage = "Không tìm thấy sản phẩm Id: " + item;
                    return View();
                }
                removeProductIdList.Add(rmItem);
            }

            _context.ProductDiscounts.RemoveRange(removeProductIdList);

            foreach (var item in addProductId)
            {
                if (await _context.Products.FindAsync(item) == null)
                {
                    StatusMessage = "Không tìm thấy sản phẩm Id: " + item;
                    return View(model);
                }

                await _context.ProductDiscounts.AddAsync(new ProductDiscount()
                {
                    DiscountId = updateDiscount.Id,
                    ProductId = item
                });
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { updateDiscount.Id });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}