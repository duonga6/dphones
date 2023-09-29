using System.ComponentModel.DataAnnotations;
using App.Areas.Products.Models;
using App.Data;
using App.Models;
using App.Models.Products;
using App.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace App.Areas.Products.Controllers
{
    [Area("Products")]
    [Authorize(Roles = RoleName.Administrator)]
    [Route("/ProductsManager/[action]")]
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ProductController> _logger;

        [TempData]
        public string? StatusMessage { set; get; }

        public ProductController(ILogger<ProductController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        private readonly int ITEM_PER_PAGE = 10;

        public IActionResult Index([FromQuery(Name = "p")] int currentPage, [FromQuery(Name = "s")] string? searchString, [FromQuery(Name = "sort")] string? sortString, [FromQuery] string? sortOrder)
        {
            var products = _context.Products.Select(p => p);
            if (searchString != null)
            {
                searchString = searchString.ToLower();
                products = products.Where(p => p.Name.ToLower().Contains(searchString) || (p.Description != null && p.Description.ToLower().Contains(searchString)) || (p.Code != null && p.Code.ToLower().Contains(searchString)));
            }

            if (string.IsNullOrEmpty(sortOrder))
            {
                sortOrder = "desc";
            }

            switch (sortString)
            {
                case "name":
                    products = sortOrder == "asc" ? products.OrderBy(p => p.Name) : products.OrderByDescending(p => p.Name);
                    break;
                case "sellprice":
                    products = sortOrder == "asc" ? products.OrderBy(p => p.SellingPrice) : products.OrderByDescending(p => p.SellingPrice);
                    break;
                case "sold":
                    products = sortOrder == "asc" ? products.OrderBy(p => p.Sold) : products.OrderByDescending(p => p.Sold);
                    break;
                default:
                    products = sortOrder == "asc" ? products.OrderBy(p => p.EntryDate) : products.OrderByDescending(p => p.EntryDate);
                    break;
            }

            ViewBag.OrderSort = sortOrder;

            ViewBag.TotalProduct = products.Count();

            int totalPage = (int)Math.Ceiling((decimal)products.Count() / ITEM_PER_PAGE);
            if (totalPage < 1) totalPage = 1;

            if (currentPage < 1)
                currentPage = 1;

            if (currentPage > totalPage)
                currentPage = totalPage;

            ViewBag.CurrentPage = currentPage;
            ViewBag.TotalPage = totalPage;

            var productInPage = products.Skip((currentPage - 1) * ITEM_PER_PAGE).Take(ITEM_PER_PAGE);

            return View(productInPage.ToList());
            // return View(products.ToList());
        }

        [HttpGet]
        public IActionResult Create()
        {
            var category = _context.Categories.ToList();
            var brand = _context.Brands.ToList();
            ViewBag.Category = new MultiSelectList(category, "Id", "Name");
            ViewBag.Brand = new SelectList(brand, "Id", "Name");
            return View();
        }

        [HttpPost, ActionName(nameof(Create))]
        public IActionResult CreateAsync([Bind("Code, Name, Slug, PurchasePrice, SellingPrice, ScreenSize, Camera, Chipset, Ram, Rom, Battery, Charger, SIM, OS, Description, Quantity, Published, EntryDate, ReleaseDate, BrandId, CategoryId")] CreateProductModel model)
        {

            model.Slug ??= AppUtilities.GenerateSlug(model.Name);
            if (_context.Products.Where(p => p.Slug == model.Slug).Any())
            {
                ModelState.AddModelError(string.Empty, "Địa chỉ Url này đã được dùng, hãy chọn địa chỉ khác");
                return View();
            }

            _context.Products.Add(model);
            _context.SaveChanges();

            if (model.CategoryId != null)
            {
                foreach (var item in model.CategoryId)
                {
                    _context.ProductCategories.Add(new ProductCategory()
                    {
                        CategoryId = item,
                        ProductId = model.Id
                    });
                }
            }
            _context.SaveChanges();
            StatusMessage = "Thêm sản phẩm thành công";
            return RedirectToAction(nameof(AddPhoto), new { Id = model.Id });
        }

        [HttpGet("{Id}")]
        public IActionResult Details(int Id)
        {
            var product = _context.Products.Where(p => p.Id == Id)
                                            .Include(p => p.ProductCategories)
                                            .ThenInclude(p => p.Category)
                                            .FirstOrDefault();

            if (product == null) return NotFound();

            return View(product);
        }

        [HttpGet("{Id}")]
        public IActionResult Edit(int Id)
        {
            var product = _context.Products.Where(p => p.Id == Id)
                                            .Include(p => p.Brand)
                                            .Include(p => p.ProductCategories)
                                            .ThenInclude(p => p.Category)
                                            .FirstOrDefault();
            if (product == null) return NotFound();

            CreateProductModel model = new()
            {
                Name = product.Name,
                Battery = product.Battery,
                Camera = product.Camera,
                Charger = product.Charger,
                Chipset = product.Chipset,
                Code = product.Code,
                Description = product.Description,
                EntryDate = product.EntryDate,
                Id = product.Id,
                OS = product.OS,
                Published = product.Published,
                PurchasePrice = product.PurchasePrice,
                Quantity = product.Quantity,
                Ram = product.Ram,
                ReleaseDate = product.ReleaseDate,
                Rom = product.Rom,
                ScreenSize = product.ScreenSize,
                SellingPrice = product.SellingPrice,
                SIM = product.SIM,
                Slug = product.Slug,
                Brand = product.Brand,
                BrandId = product.BrandId,
                CategoryId = product.ProductCategories.Select(p => p.CategoryId).ToArray()
            };

            var category = _context.Categories.ToList();
            ViewBag.Categories = new MultiSelectList(category, "Id", "Name");
            var brand = _context.Brands.ToList();
            ViewBag.Brands = new SelectList(brand, "Id", "Name");
            return View(model);
        }

        [HttpPost("{Id}"), ActionName(nameof(Edit))]
        [ValidateAntiForgeryToken]
        public IActionResult EditAsync(int Id, [Bind("Code, Name, Slug, PurchasePrice, SellingPrice, ScreenSize, Camera, Chipset, Ram, Rom, Battery, Charger, SIM, OS, Description, Quantity, Published, EntryDate, ReleaseDate, BrandId, CategoryId")] CreateProductModel model)
        {
            var category = _context.Categories.ToList();
            ViewBag.Categories = new MultiSelectList(category, "Id", "Name");

            var brand = _context.Brands.ToList();
            ViewBag.Brands = new SelectList(brand, "Id", "Name");

            if (!ModelState.IsValid) return View();

            var productUpdate = _context.Products.Where(p => p.Id == Id).Include(p => p.ProductCategories).FirstOrDefault();
            if (productUpdate == null) return NotFound();

            model.Slug ??= AppUtilities.GenerateSlug(model.Name);
            if (_context.Products.Where(p => p.Slug == model.Slug && p.Id == model.Id).Any())
            {
                ModelState.AddModelError(string.Empty, "Địa chỉ Url này đã được dùng, hãy chọn địa chỉ khác");
                return View();
            }

            try
            {
                productUpdate.Name = model.Name;
                productUpdate.Battery = model.Battery;
                productUpdate.Camera = model.Camera;
                productUpdate.Charger = model.Charger;
                productUpdate.Chipset = model.Chipset;
                productUpdate.Code = model.Code;
                productUpdate.Description = model.Description;
                productUpdate.EntryDate = model.EntryDate;
                productUpdate.OS = model.OS;
                productUpdate.Published = model.Published;
                productUpdate.PurchasePrice = model.PurchasePrice;
                productUpdate.Quantity = model.Quantity;
                productUpdate.Ram = model.Ram;
                productUpdate.ReleaseDate = model.ReleaseDate;
                productUpdate.Rom = model.Rom;
                productUpdate.ScreenSize = model.ScreenSize;
                productUpdate.SellingPrice = model.SellingPrice;
                productUpdate.SIM = model.SIM;
                productUpdate.Slug = model.Slug;
                productUpdate.BrandId = model.BrandId;

                var oldCate = productUpdate.ProductCategories.Select(p => p);
                var newCate = model.CategoryId;
                newCate ??= new int[] { };

                var addCate = newCate.Where(c => !oldCate.Where(ct => ct.CategoryId == c).Any());
                var removeCate = oldCate.Where(p => !newCate.Contains(p.CategoryId));

                _context.ProductCategories.RemoveRange(removeCate);

                foreach (var item in addCate)
                {
                    _context.ProductCategories.Add(new ProductCategory
                    {
                        CategoryId = item,
                        ProductId = Id
                    });
                }

                _context.SaveChanges();

            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Products.Any(p => p.Id == Id))
                    return NotFound();
            }

            StatusMessage = "Cập nhật thành công";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult AddPhoto(int Id)
        {
            var product = _context.Products.Where(p => p.Id == Id).FirstOrDefault();
            if (product == null)
            {
                StatusMessage = "Có lỗi khi thêm sản phẩm";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Product = product;

            return View();
        }

        [HttpGet("{Id}")]
        public IActionResult Delete(int Id)
        {
            var product = _context.Products.Where(p => p.Id == Id).FirstOrDefault();
            if (product == null) return NotFound();

            return View(product);
        }

        [HttpPost("{Id}"), ActionName(nameof(Delete))]
        public async Task<IActionResult> DeleteAsync(int Id)
        {
            var product = await _context.Products.Where(p => p.Id == Id).FirstOrDefaultAsync();
            if (product == null) return NotFound();

            var photos = await _context.ProductPhotos.Where(p => p.ProductId == Id).ToListAsync();
            if (photos != null) _context.ProductPhotos.RemoveRange(photos);

            _context.Products.Remove(product);

            await _context.SaveChangesAsync();

            StatusMessage = "Xóa thành công";
            return RedirectToAction(nameof(Index));
        }

        public class UploadOneFile
        {
            [Display(Name = "Chọn hình ảnh")]
            [DataType(DataType.Upload)]
            [FileExtensions(Extensions = "png, jpg, jpeg, webp")]
            public IFormFile? FileUpload { set; get; }
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult GetPhoto(int Id)
        {
            var product = _context.Products.Where(p => p.Id == Id).FirstOrDefault();
            if (product == null) return NotFound();

            var mainPhoto = _context.ProductPhotos.Where(p => p.Id == product.MainPhoto).Select(p => new
            {
                id = p.Id,
                filename = p.Name
            }).FirstOrDefault();

            var subPhoto = _context.ProductPhotos.Where(p => p.ProductId == product.Id && p.Id != product.MainPhoto).Select(p => new
            {
                id = p.Id,
                filename = p.Name
            }).ToList();

            return Json(new
            {
                mainPhoto,
                subPhoto
            });
        }

        [HttpPost]
        public async Task<IActionResult> UploadImage(int Id, [Bind("FileUpload")] UploadOneFile f, string type)
        {
            var product = _context.Products.Where(p => p.Id == Id).FirstOrDefault();
            if (product == null) return NotFound("Không tìm thấy sản phẩm này");

            var file = f.FileUpload;

            if (file != null && file.Length > 0)
            {
                var filename = Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + Path.GetExtension(file.FileName);
                var filePath = Path.Combine("Uploads", "Products", filename);

                using var fileStream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(fileStream);

                var photo = new ProductPhoto()
                {
                    Name = filename,
                    Product = product
                };

                _context.ProductPhotos.Add(photo);
                _context.SaveChanges();

                if (type == "main") product.MainPhoto = photo.Id;

                _context.SaveChanges();
            }

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteImage(int Id, int productid, string type)
        {
            var product = _context.Products.Where(p => p.Id == productid).FirstOrDefault();
            if (product == null) return NotFound();

            var photo = _context.ProductPhotos.Where(p => p.Id == Id).FirstOrDefault();
            if (photo == null) return NotFound();

            string filepath = Path.Combine("Uploads", "Products", photo.Name);
            if (System.IO.File.Exists(filepath))
            {
                System.IO.File.Delete(filepath);
            }

            if (type == "main") product.MainPhoto = null;
            _context.ProductPhotos.Remove(photo);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}