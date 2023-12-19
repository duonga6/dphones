using System.Text;
using System.Text.Encodings.Web;
using App.Areas.Identity.Models.Manage;
using App.Models;
using App.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace App.Areas.Identity.Controllers
{
    [Area("Identity")]
    [Authorize]
    [Route("/User/[action]")]
    public class ManageController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<AccountController> _logger;

        public ManageController(UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            IEmailSender emailSender,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _logger = logger;
        }

        [TempData]
        public string? StatusMessage { set; get; }

        // Thông tin user
        [HttpGet("/user")]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound("Không tìm thấy user");

            ViewBag.Email = user.Email;
            ViewBag.IsConfirmedEmail = user.EmailConfirmed;

            var model = new IndexViewModel
            {
                UserName = user.UserName,
                PhoneNumber = user.PhoneNumber,
                FullName = user.FullName,
                BirthDate = user.BirthDate,
                HomeAddress = user.HomeAddress,
                UserAvatar = user.UserAvatar
            };

            return View(model);
        }

        // Cập nhật thông tin
        [HttpPost("/user")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(IndexViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound("Không tìm thấy user");

            user.PhoneNumber = model.PhoneNumber;
            user.FullName = model.FullName ?? "";
            user.BirthDate = model.BirthDate;
            user.HomeAddress = model.HomeAddress;

            var file = model.ImageFile;
            if (file != null && file.Length > 0)
            {
                var filename = Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + Path.GetExtension(file.FileName);
                var filepath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "UserAvatar", filename);
                using var filestream = new FileStream(filepath, FileMode.Create);
                await file.CopyToAsync(filestream);

                user.UserAvatar = filename;
            }

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                StatusMessage = "Cập nhật thành công";
                await _signInManager.RefreshSignInAsync(user);
            }
            else
            {
                StatusMessage = "Cập nhật thất bại";
            }

            return RedirectToAction(nameof(Index));
        }

        // Gửi lại email xác nhận
        [HttpGet]
        public async Task<IActionResult> ResendEmailConfirmed()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("Không tìm thấy user");
            }

            if (await _userManager.IsEmailConfirmedAsync(user))
            {
                StatusMessage = "Email đã được xác minh rồi";
                return RedirectToAction(nameof(Index));
            }

            var userId = await _userManager.GetUserIdAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.ActionLink(
                action: "ConfirmedEmail",
                controller: "Account",
                values: new { area = "Identity", userId = userId, code = code },
                protocol: Request.Scheme);

            string emailHtml = AppUtilities.GenerateHtmlEmail(user.FullName, HtmlEncoder.Default.Encode(callbackUrl ?? ""));
            await _emailSender.SendEmailAsync(user.Email!, "Xác thực tài khoản", emailHtml);

            StatusMessage = "Email xác thực đã được gửi, vui lòng kiểm tra email để xác thực.";
            return RedirectToAction(nameof(Index));
        }

        // Đổi mk
        [HttpGet]
        public async Task<IActionResult> ChangePassword()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound("Không tìm thấy user");
            ViewBag.User = user;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePasswordAsync(ChangePasswordModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound("Không tìm thấy user");
            ViewBag.User = user;

            if (!ModelState.IsValid) return View(model);

            if (model.Password == model.NewPassword)
            {
                ModelState.AddModelError(string.Empty, "Mật khẩu mới phải khác mật khẩu ban đầu");
                return View(model);
            }

            bool checkOldPassword = await _userManager.CheckPasswordAsync(user, model.Password);

            if (!checkOldPassword)
            {
                ModelState.AddModelError(string.Empty, "Mật khẩu cũ không đúng");
                return View(model);
            }

            await _userManager.RemovePasswordAsync(user);
            var result = await _userManager.AddPasswordAsync(user, model.NewPassword);
            if (result.Succeeded)
            {
                StatusMessage = "Thay đổi mật khẩu thành công";
            }
            else
            {
                foreach (var item in result.Errors)
                    ModelState.AddModelError(string.Empty, item.Description);
                return View();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}