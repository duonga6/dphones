using App.Data;
using App.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Bogus;
using App.Models.Products;
using Bogus.DataSets;

namespace App.Areas.Database.Controllers
{
    [Area("Database")]
    [Route("/database-manager/[action]")]
    public class DbManagerController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<DbManagerController> _logger;

        public DbManagerController(AppDbContext context,
                                    UserManager<AppUser> userManager,
                                    SignInManager<AppUser> signInManager,
                                    RoleManager<IdentityRole> roleManager,
                                    ILogger<DbManagerController> logger)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        [TempData]
        public string? StatusMessage { set; get; }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> SeedData()
        {
            var roleNames = typeof(RoleName).GetFields().ToList();
            foreach (var r in roleNames)
            {
                var roleName = (string)(r.GetRawConstantValue() ?? "");
                var roleFound = await _roleManager.FindByNameAsync(roleName);
                if (roleFound == null)
                    await _roleManager.CreateAsync(new IdentityRole(roleName));
            }

            SeedProduct();

            StatusMessage = "Seed data thành công";
            return RedirectToAction(nameof(Index));
        }

        public void SeedProduct()
        {
            var productWithOutFake = _context.Products.Where(b => b.Description == null || !b.Description.Contains("[FakeData]")).ToList();
            foreach (var item in productWithOutFake)
                item.BrandId = null;
            _context.Products?.RemoveRange(_context.Products.Where(b => b.Description != null && b.Description.Contains("[FakeData]")).ToList());
            _context.Brands?.RemoveRange(_context.Brands.Where(b => b.Description != null && b.Description.Contains("[FakeData]")).ToList());
            _context.Categories?.RemoveRange(_context.Categories.Where(b => b.Description != null && b.Description.Contains("[FakeData]")).ToList());
            _context.SaveChanges();

            var fakerBrand = new Faker<Brand>();
            fakerBrand.RuleFor(b => b.Name, f => f.Lorem.Sentence(1, 2).Trim('.'));
            fakerBrand.RuleFor(b => b.Description, f => f.Lorem.Sentence(20) + "[FakeData]");
            fakerBrand.RuleFor(b => b.Slug, f => f.Random.Replace("***-***-****-***-***").ToLower());

            var fakerCategory = new Faker<Category>();
            fakerCategory.RuleFor(c => c.Name, f => f.Lorem.Sentence(1, 2).Trim('.'));
            fakerCategory.RuleFor(c => c.Description, f => f.Lorem.Sentence(20) + "[FakeData]");
            fakerCategory.RuleFor(c => c.Slug, f => f.Random.Replace("***-**-****-**-***").ToLower());

            var fakerProduct = new Faker<Product>();
            fakerProduct.RuleFor(p => p.Code, f => f.Random.Replace("***-***-**"));
            fakerProduct.RuleFor(p => p.Name, f => f.Lorem.Sentence(1, 2).Trim('.'));
            fakerProduct.RuleFor(p => p.Slug, f => f.Random.Replace("*-***-**-***-***").ToLower());
            fakerProduct.RuleFor(p => p.PurchasePrice, f => decimal.Round(f.Random.Decimal(20, 340), 0) * 100000);
            fakerProduct.RuleFor(p => p.SellingPrice, f => decimal.Round(f.Random.Decimal(30, 440), 0) * 100000);
            fakerProduct.RuleFor(p => p.ScreenSize, f => double.Round(f.Random.Double(4, 8), 1));
            fakerProduct.RuleFor(p => p.Camera, f => f.Lorem.Sentence(4, 8));
            fakerProduct.RuleFor(p => p.Chipset, f => f.Lorem.Sentence(1, 2));
            fakerProduct.RuleFor(p => p.Battery, f => f.Random.Int(3125, 8000));
            fakerProduct.RuleFor(p => p.Charger, f => f.Random.Int(15, 180));
            fakerProduct.RuleFor(p => p.SIM, f => f.Lorem.Sentence(3, 7));
            fakerProduct.RuleFor(p => p.OS, f => f.Lorem.Sentence(3, 7));
            fakerProduct.RuleFor(p => p.Description, f => f.Lorem.Paragraph(4) + "[FakeData]");
            fakerProduct.RuleFor(p => p.Quantity, f => f.Random.Int(1, 100));
            fakerProduct.RuleFor(p => p.Sold, f => f.Random.Int(0, 10));
            fakerProduct.RuleFor(p => p.Published, f => true);
            fakerProduct.RuleFor(p => p.ReleaseDate, f => f.Date.Between(new DateTime(2020, 1, 1), new DateTime(2024, 1, 1)));
            fakerProduct.RuleFor(p => p.EntryDate, f => f.Date.Between(new DateTime(2020, 1, 1), new DateTime(2024, 1, 1)));


            List<Brand> brands = new();
            List<Category> categories = new();
            List<Product> products = new();
            List<ProductCategory> productCategories = new();

            int brandsQtt = 10;
            int categoryQtt = 8;
            int productQtt = 100;

            for (int i = 1; i <= brandsQtt; i++)
            {
                brands.Add(fakerBrand.Generate());
            }

            for (int i = 1; i <= categoryQtt; i++)
            {
                categories.Add(fakerCategory.Generate());
            }

            var random = new Random();
            for (int i = 1; i <= productQtt; i++)
            {
                var product = fakerProduct.Generate();
                product.Brand = brands[random.Next(0, brandsQtt - 1)];
                products.Add(product);

                productCategories.Add(new ProductCategory()
                {
                    Product = product,
                    Category = categories[random.Next(0, categoryQtt - 1)]
                });
            }

            _context.Brands?.AddRange(brands);
            _context.Categories?.AddRange(categories);
            _context.Products?.AddRange(products);
            _context.ProductCategories?.AddRange(productCategories);
            _context.SaveChanges();

        }

    }
}