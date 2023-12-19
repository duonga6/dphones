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
            // Lấy ds backup
            string backUpPath = Path.Combine(Directory.GetCurrentDirectory(), "BackupDB");
            var filename = Directory.GetFiles(backUpPath).ToList();
            ViewBag.BackUpList = filename.Select(f => Path.GetFileName(f)).ToList();
            return View();
        }


        // Áp dụng migration cuối
        [Route("/database-manager/migration")]
        [AllowAnonymous]
        public async Task<IActionResult> Migrations()
        {
            await _context.Database.MigrateAsync();
            StatusMessage = "Cập nhật thành công";
            return RedirectToAction(nameof(Index));
        }

        // Tạo data ảo, tk admin
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
            useradmin ??= await _userManager.FindByEmailAsync("admin@gmail.com");
            if (useradmin == null)
            {
                var user = new AppUser
                {
                    FullName = "admin",
                    Email = "admin@gmail.com",
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

            StatusMessage = "Seed data thành công";
            return RedirectToAction(nameof(Index));
        }

        // Tạo backup db
        [Route("/database-manager/backup-db")]
        public async Task<IActionResult> BackUpDBAsync(string info)
        {
            string now = DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss");
            string path = Path.Combine(Directory.GetCurrentDirectory(), "BackupDB", $"backup_file_{now}_{info}.bak");

            FormattableString script = $@"
                BACKUP DATABASE dphones TO DISK = {path}
            ";

            await _context.Database.ExecuteSqlInterpolatedAsync(script);

            return RedirectToAction(nameof(Index));
        }

        // Phục hồi DB
        [Route("/database-manager/restore-db/{fileName}")]
        public async Task<IActionResult> RestoreDBAsync(string fileName)
        {
            string backUpPath = Path.Combine(Directory.GetCurrentDirectory(), "BackupDB"); ;
            var nameList = Directory.GetFiles(backUpPath).Select(n => Path.GetFileName(n)).ToList();
            string path = Path.Combine(Directory.GetCurrentDirectory(), "BackupDB", fileName);

            if (!nameList.Contains(fileName))
            {
                StatusMessage = $"Không tìm thấy bản Backup này";
            }
            else
            {
                FormattableString script = $@"
                    ALTER DATABASE dphones
                    SET OFFLINE WITH ROLLBACK IMMEDIATE

                    DECLARE @mdfPath NVARCHAR(255);
                    DECLARE @ldfPath NVARCHAR(255);

                    SELECT @mdfPath = physical_name
                    FROM sys.master_files
                    WHERE name = 'dphones';

                    SELECT @ldfPath = physical_name
                    FROM sys.master_files
                    WHERE name = 'dphones_log';

                    RESTORE DATABASE dphones 
                    FROM DISK = {path}
                    WITH REPLACE,
                    MOVE 'dphones' TO @mdfPath,
                    MOVE 'dphones_log' TO @ldfPath;

                    ALTER DATABASE dphones
                    SET ONLINE
                ";
                await _context.Database.ExecuteSqlInterpolatedAsync(script);

                StatusMessage = $"Phục hồi thành công";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}