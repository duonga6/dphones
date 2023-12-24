using System.Text;
using System.Text.Encodings.Web;
using App.Areas.Identity.Models.Account;
using App.Data;
using App.Models;
using App.Utilities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace App.Areas.Identity.Controllers
{
    [Authorize]
    [Area("Identity")]
    [Route("/Account/[action]")]
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<AccountController> _logger;

        public AccountController(UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            IEmailSender emailSender,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _logger = logger;
        }

        [HttpGet("/login/")]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["returnUrl"] = returnUrl;
            return View();
        }

        [HttpPost("/login/")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ViewData["returnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                // Login bằng username
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);
                if (result.Succeeded)
                {
                    _logger.LogInformation($"{DateTime.Now} User {model.Email} đã đăng nhập");
                    return LocalRedirect(returnUrl);
                }
                else if (result.IsLockedOut)
                {
                    ModelState.AddModelError(string.Empty, "Tài khoản này đang bị khóa.");
                    return View(model);
                }
                else
                {
                    // Login bằng email
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    if (user != null && user.UserName != null)
                    {
                        var results = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, model.RememberMe, lockoutOnFailure: true);
                        if (results.Succeeded)
                        {
                            _logger.LogInformation($"{DateTime.Now} User {user.UserName} đã đăng nhập");
                            return LocalRedirect(returnUrl);
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Thông tin tài khoản hoặc mật khẩu không chính xác");
                            return View(model);
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Thông tin tài khoản hoặc mật khẩu không chính xác");
                        return View(model);
                    }
                }
            }

            return View(model);
        }


        [ValidateAntiForgeryToken]
        [HttpPost("/logout")]
        public async Task<IActionResult> LogOff()
        {
            var user = await _userManager.GetUserAsync(User);
            await _signInManager.SignOutAsync();
            _logger.LogInformation($"{DateTime.Now} User {user?.UserName} đã đăng xuất");
            return RedirectToAction("Index", "Home");
        }

        [HttpGet("/khongduoctruycap")]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet("/forgot-password")]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost("/forgot-password")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToAction(nameof(ForgotPasswordConfirm));
                }

                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.ActionLink(
                    action: nameof(ResetPassword),
                    values: new { area = "Identity", code, email = user.Email },
                    protocol: Request.Scheme);

                string emailContent = @$"
Bạn đã thực hiện yêu cầu đặt lại mật khẩu. Hãy <a href='{HtmlEncoder.Default.Encode(callbackUrl ?? "")}'>ấn vào đây</a> để đặt lại mật khẩu.

Xin cảm ơn.
                ";

                string emailHtml = AppUtilities.GenerateHtmlEmail(user.FullName, emailContent);

                await _emailSender.SendEmailAsync(
                    model.Email,
                    "Đặt lại mật khẩu",
                    emailContent);

                return RedirectToAction(nameof(ForgotPasswordConfirm));
            }

            return View();
        }

        [AllowAnonymous]
        [HttpGet("/forgot-password-confirmed")]
        public IActionResult ForgotPasswordConfirm()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpGet("/reset-password")]
        public IActionResult ResetPassword(string? code, string? email)
        {
            return code == null ? NotFound() : View();
        }

        [AllowAnonymous]
        [HttpPost("/reset-password")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction(nameof(ResetPasswordConfirmed));
            }
            var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Code));
            var result = await _userManager.ResetPasswordAsync(user, code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(ResetPasswordConfirmed));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        [AllowAnonymous]
        [HttpGet("/reset-password-confirmed")]
        public IActionResult ResetPasswordConfirmed()
        {
            return View();
        }

        [HttpGet("/register")]
        [AllowAnonymous]
        public IActionResult Register(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ViewData["returnUrl"] = returnUrl;
            return View();
        }

        [HttpPost("/register")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterModel model, string? returnUrl)
        {
            returnUrl ??= Url.Content("~/");
            if (ModelState.IsValid)
            {
                var user = new AppUser
                {
                    UserName = model.UserName,
                    FullName = model.FullName,
                    Email = model.Email
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, RoleName.Customer);
                    _logger.LogInformation($"Tài khoản {model.UserName} được đăng ký.");

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.ActionLink(
                        action: nameof(ConfirmedEmail),
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    string emailContent = @$"
Chúc mừng, bạn đã đang ký tài khoản thành công. Hãy <a href='{HtmlEncoder.Default.Encode(callbackUrl ?? "")}'>ấn vào đây</a> để xác thực tài khoản.

Xin cảm ơn.
                    ";

                    string emailHtml = AppUtilities.GenerateHtmlEmail(user.FullName, emailContent);

                    await _emailSender.SendEmailAsync(model.Email, "Đăng ký thành công", emailHtml);

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToAction(nameof(RegisterConfirmation), new { email = model.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return View();
        }

        [AllowAnonymous]
        [HttpGet("/confirmed-email")]
        public async Task<IActionResult> ConfirmedEmail(string userId, string code)
        {
            if (userId == null || code == null) return RedirectToAction("Index", "Home");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound("Lỗi: user không tồn tại");
            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ConfirmEmailAsync(user, code);
            return result.Succeeded ? View() : NotFound("Lỗi không thể xác thực email được");
        }

        [HttpGet("/register-confirmed")]
        public IActionResult RegisterConfirmation()
        {
            return View();
        }

        [HttpGet("/resend-email-confirm")]
        [AllowAnonymous]
        public IActionResult ResendEmailConfirmation()
        {
            return View();
        }

        [HttpPost("/resend-email-confirm")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendEmailConfirmation(ResendEmailModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Email xác thực đã được gửi, vui lòng kiểm tra email để xác thực.");
                return View(model);
            }

            if (await _userManager.IsEmailConfirmedAsync(user))
            {
                ModelState.AddModelError(string.Empty, "Email xác thực đã được gửi, vui lòng kiểm tra email để xác thực.");
                return View(model);
            }

            var userId = await _userManager.GetUserIdAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.ActionLink(
                action: nameof(ConfirmedEmail),
                values: new { userId = userId, code = code },
                protocol: Request.Scheme);

            string emailContent = @$"
Bạn đã yêu cầu gửi lại email xác thực. Hãy <a href='{HtmlEncoder.Default.Encode(callbackUrl ?? "")}'>ấn vào đây</a> để xác thực tài khoản.

Xin cảm ơn.
            ";

            string emailHtml = AppUtilities.GenerateHtmlEmail(user.FullName, emailContent);

            await _emailSender.SendEmailAsync(
                model.Email,
                "Xác thực tài khoản",
                emailHtml);

            ModelState.AddModelError(string.Empty, "Email xác thực đã được gửi, vui lòng kiểm tra email để xác thực.");
            return View(model);
        }
    }

}