using System.IO;
using App.Data;
using App.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Bogus;
using App.Models.Products;
using Bogus.DataSets;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.SqlClient;

namespace App.Areas.Database.Controllers
{
    [Area("Database")]
    // [Authorize(Roles = RoleName.Administrator)]
    public class DbManagerController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<DbManagerController> _logger;
        private readonly IConfiguration _configuration;

        public DbManagerController(AppDbContext context,
                                    UserManager<AppUser> userManager,
                                    SignInManager<AppUser> signInManager,
                                    RoleManager<IdentityRole> roleManager,
                                    ILogger<DbManagerController> logger,
                                    IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _logger = logger;
            _configuration = configuration;
        }

        [TempData]
        public string? StatusMessage { set; get; }

        [Route("/database-manager/index")]
        [AllowAnonymous]
        public IActionResult Index()
        {
            string backUpPath = Path.Combine(Directory.GetCurrentDirectory(), "BackupDB");

            var filename = Directory.GetFiles(backUpPath).ToList();

            ViewBag.BackUpList = filename.Select(f => Path.GetFileName(f)).ToList();
            return View();
        }

        [Route("/database-manager/migration")]
        [AllowAnonymous]
        public async Task<IActionResult> Migrations()
        {
            await _context.Database.MigrateAsync();
            StatusMessage = "Cập nhật thành công";
            return RedirectToAction(nameof(Index));
        }

        [AllowAnonymous]
        [Route("/database-manager/seed-data")]
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

            var useradmin = await _userManager.FindByNameAsync("admin");
            useradmin ??= await _userManager.FindByEmailAsync("trieuvip14@gmail.com");
            if (useradmin == null)
            {
                var user = new AppUser
                {
                    FullName = "admin",
                    Email = "trieuvip14@gmail.com",
                    EmailConfirmed = true,
                    UserName = "admin"
                };

                await _userManager.CreateAsync(user, "admin");
                await _userManager.AddToRoleAsync(user, RoleName.Administrator);
                await _signInManager.SignInAsync(user, false);

            }
            else
            {
                await _userManager.DeleteAsync(useradmin);
                return RedirectToAction(nameof(SeedData));
            }

            // SeedProduct();

            StatusMessage = "Seed data thành công";
            return RedirectToAction(nameof(Index));
        }

        [Route("/database-manager/backup-db")]
        public IActionResult BackUpDB(string info)
        {
            string now = DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss");
            string path = Path.Combine(Directory.GetCurrentDirectory(), "BackupDB", $"backup_file_{now}_{info}.bak");
            var p = new Process
            {
                StartInfo =
                {
                    FileName = Environment.OSVersion.Platform == PlatformID.Unix ? "/opt/mssql-tools/bin/sqlcmd" : "sqlcmd",
                    WorkingDirectory = Directory.GetCurrentDirectory(),
                    Arguments = $"-S localhost -U sa -P 12345678Aa -Q \"BACKUP DATABASE dphones TO DISK = '{path}'\""
                }
            };

            p.Start();

            p.WaitForExit();

            if (p.ExitCode != 0)
            {
                StatusMessage = $"BackUp thất bại \n {p.StandardError.ReadToEnd()}";
            }



            return RedirectToAction(nameof(Index));
        }

        [Route("/database-manager/restore-db/{fileName}")]
        public IActionResult RestoreDB(string fileName)
        {
            string backUpPath = Path.Combine(Directory.GetCurrentDirectory(), "BackupDB");;
            var nameList = Directory.GetFiles(backUpPath).Select(n => Path.GetFileName(n)).ToList();
            string path = Path.Combine(Directory.GetCurrentDirectory(), "BackupDB", fileName);

            if (!nameList.Contains(fileName))
            {
                StatusMessage = $"Không tìm thấy bản Backup này";
            }
            else
            {
                string script = $@"
                    ALTER DATABASE dphones
                    SET OFFLINE WITH ROLLBACK IMMEDIATE
                    GO

                    DECLARE @mdfPath NVARCHAR(255);
                    DECLARE @ldfPath NVARCHAR(255);

                    SELECT @mdfPath = physical_name
                    FROM sys.master_files
                    WHERE name = 'dphones';

                    SELECT @ldfPath = physical_name
                    FROM sys.master_files
                    WHERE name = 'dphones_log';

                    RESTORE DATABASE dphones 
                    FROM DISK = '{path}' 
                    WITH REPLACE,
                    MOVE 'dphones' TO @mdfPath,
                    MOVE 'dphones_log' TO @ldfPath;
                    GO

                    ALTER DATABASE dphones
                    SET ONLINE
                    GO
                ";
                var p = new Process
                {
                    StartInfo =
                {
                    FileName = Environment.OSVersion.Platform == PlatformID.Unix ? "/opt/mssql-tools/bin/sqlcmd" : "sqlcmd",
                    WorkingDirectory = Directory.GetCurrentDirectory(),
                    Arguments = $"-S localhost -U sa -P 12345678Aa -d master -Q \"{script}\""
                }
                };

                p.Start();

                p.WaitForExit();

                if (p.ExitCode != 0)
                {
                    StatusMessage = $"Phục hồi thất bại \n {p.StandardError.ReadToEnd()}";
                }

                StatusMessage = $"Phục hồi thành công";
            }

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
            fakerProduct.RuleFor(p => p.ScreenSize, f => double.Round(f.Random.Double(4, 8), 1));
            fakerProduct.RuleFor(p => p.Camera, f => f.Lorem.Sentence(4, 8));
            fakerProduct.RuleFor(p => p.Chipset, f => f.Lorem.Sentence(1, 2));
            fakerProduct.RuleFor(p => p.Battery, f => f.Random.Int(3125, 8000));
            fakerProduct.RuleFor(p => p.Charger, f => f.Random.Int(15, 180));
            fakerProduct.RuleFor(p => p.SIM, f => f.Lorem.Sentence(3, 7));
            fakerProduct.RuleFor(p => p.OS, f => f.Lorem.Sentence(3, 7));
            fakerProduct.RuleFor(p => p.Description, f => f.Lorem.Paragraph(4) + "[FakeData]");
            fakerProduct.RuleFor(p => p.Published, f => true);
            fakerProduct.RuleFor(p => p.ReleaseDate, f => f.Date.Between(new DateTime(2020, 1, 1), new DateTime(2024, 1, 1)));
            fakerProduct.RuleFor(p => p.EntryDate, f => f.Date.Between(new DateTime(2020, 1, 1), new DateTime(2024, 1, 1)));


            List<Brand> brands = new();
            List<Category> categories = new();
            List<Product> products = new();
            List<ProductCategory> productCategories = new();

            int brandsQtt = 5;
            int categoryQtt = 8;
            int productQtt = 30;

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