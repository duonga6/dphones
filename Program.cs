using App.Areas.Products.Services;
using App.Data;
using App.Models;
using App.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using SignalRChat.Hubs;

var builder = WebApplication.CreateBuilder(args);



//=========================== Start Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddOptions();
builder.Services.AddHttpClient();
builder.Services.AddSignalR();
builder.Services.AddSingleton<ConnectionManagerService>();

builder.WebHost.UseUrls("http://0.0.0.0:8090");

// Database
var connectionString = builder.Configuration.GetConnectionString("AppDbContext");
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});

// Identity
builder.Services.AddIdentity<AppUser, IdentityRole>()
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// Cache + Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(cfg =>
{
    cfg.Cookie.Name = "gio-hang";
    cfg.IdleTimeout = new TimeSpan(1, 0, 0, 0);
});

builder.Services.AddHttpContextAccessor();

// IdentityOptions
builder.Services.Configure<IdentityOptions>(options =>
{
    // Thiết lập về Password
    options.Password.RequireDigit = false; // Không bắt phải có số
    options.Password.RequireLowercase = false; // Không bắt phải có chữ thường
    options.Password.RequireNonAlphanumeric = false; // Không bắt ký tự đặc biệt
    options.Password.RequireUppercase = false; // Không bắt buộc chữ in
    options.Password.RequiredLength = 3; // Số ký tự tối thiểu của password
    options.Password.RequiredUniqueChars = 0; // Số ký tự riêng biệt

    // Cấu hình Lockout - khóa user
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); // Khóa 5 phút
    options.Lockout.MaxFailedAccessAttempts = 5; // Thất bại 5 lầ thì khóa
    options.Lockout.AllowedForNewUsers = false;

    // Cấu hình về User.
    options.User.AllowedUserNameCharacters = // các ký tự đặt tên user
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;  // Email là duy nhất

    // Cấu hình đăng nhập.
    options.SignIn.RequireConfirmedEmail = false;            // Cấu hình xác thực địa chỉ email (email phải tồn tại)
    options.SignIn.RequireConfirmedPhoneNumber = false;     // Xác thực số điện thoại

});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/login"; // Trang đăng nhập
    options.LogoutPath = "/logout"; // Trang đăng xuất
    options.AccessDeniedPath = "/khongduoctruycap"; // Trang cấm truy cập
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ViewMenuAdmin", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireRole(RoleName.Administrator);
    });
});

// Mail Sender
var mailSettings = builder.Configuration.GetSection("MailSettings");
builder.Services.Configure<MailSettings>(mailSettings);
builder.Services.AddTransient<IEmailSender, SendMailService>();

// VnPay
var vnPaySettings = builder.Configuration.GetSection("VnPaySettings");
builder.Services.Configure<VnPaySettings>(vnPaySettings);
builder.Services.AddTransient<VnPayService>();

builder.Services.AddRazorPages();
builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
builder.Services.AddTransient<SidebarAdminService>();


// Cart
builder.Services.AddTransient<CartService>();

// Home Category
builder.Services.AddTransient<HomeCategoryService>();


//=========================== End Add services to the container.

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions()
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "Uploads")),
    RequestPath = "/files"
});

app.UseRouting();
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();
app.MapHub<ChatHub>("/chatHub");





using (var scopeService = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
{
    var _context = scopeService.ServiceProvider.GetService<AppDbContext>();
    if (_context != null && _context.Database.GetPendingMigrations().Any())
    {
        _context.Database.Migrate();
    }
}

app.Run();
