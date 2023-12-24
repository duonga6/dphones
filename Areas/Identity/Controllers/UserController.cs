using System.Net.Sockets;
using System.Security.Claims;
using App.Areas.Identity.Models.User;
using App.Data;
using App.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace App.Areas.Identity.Controllers
{
    [Area("Identity")]
    [Route("/ManagerUser/[action]")]
    [Authorize(Roles = RoleName.Administrator)]
    public class UserController : Controller
    {
        private readonly UserManager<AppUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        private readonly AppDbContext _context;

        private readonly ILogger<UserController> _logger;

        [TempData]
        public string? StatusMessage { set; get; }

        private readonly int ITEM_PER_PAGE = 10;

        public UserController(ILogger<UserController> logger, AppDbContext context, RoleManager<IdentityRole> roleManager, UserManager<AppUser> userManager)
        {
            _logger = logger;
            _context = context;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index([FromQuery(Name = "p")] int currentPage, [FromQuery(Name = "s")] string? searchString)
        {
            var users = _context.Users
            .AsNoTracking()
            .OrderBy(u => u.UserName)
            .AsQueryable();

            if (searchString != null)
            {
                searchString = searchString.ToLower();
                users = users.Where(u => u.UserName!.ToLower().Contains(searchString) || u.Email!.ToLower().Contains(searchString));
            }

            int pageCount = (int)Math.Ceiling((double)users.Count() / ITEM_PER_PAGE);

            if (pageCount < 1)
                pageCount = 1;

            if (currentPage < 1)
                currentPage = 1;

            if (currentPage > pageCount)
                currentPage = pageCount;

            ViewBag.CurrentPage = currentPage;
            ViewBag.CountPage = pageCount;

            var userList = users.Select(u => new UserModel
            {
                FullName = u.FullName,
                UserName = u.UserName,
                Email = u.Email,
                Id = u.Id,
                EmailConfirmed = u.EmailConfirmed,
                LockoutEnabled = u.LockoutEnabled,
                LockoutEnd = u.LockoutEnd,
                PhoneNumber = u.PhoneNumber
            });

            var model = new UserIndexModel
            {
                TotalUser = users.Count(),
                User = userList.Skip((currentPage - 1) * ITEM_PER_PAGE).Take(ITEM_PER_PAGE).ToList()
            };

            foreach (var user in model.User)
            {
                user.Roles = (await _userManager.GetRolesAsync(user)).ToList();
            }

            return View(model);
        }

        [HttpGet("{userid}")]
        public async Task<IActionResult> AddRole(string userid)
        {
            var user = await _userManager.FindByIdAsync(userid);
            if (user == null) return NotFound("Không tìm thấy user");

            var model = new AddRoleUserModel()
            {
                User = user,
                RolesName = (await _userManager.GetRolesAsync(user)).ToList()
            };

            var allRoles = _context.Roles.AsNoTracking()
            .Select(r => r.Name).OrderBy(r => r).ToList();
            ViewBag.allRoles = new SelectList(allRoles);

            GetClaims(model);

            return View(model);
        }

        [HttpPost("{userid}"), ActionName(nameof(AddRole))]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRoleAsync(string userid, AddRoleUserModel model)
        {
            var user = await _userManager.FindByIdAsync(userid);
            if (user == null) return NotFound("Không tìm thấy user");

            model.User = user;

            var allRoles = _context.Roles.Select(r => r.Name).OrderBy(r => r).ToList();
            ViewBag.allRoles = new SelectList(allRoles);
            GetClaims(model);

            var oldRoles = await _userManager.GetRolesAsync(user);
            var rolesAdd = model.RolesName?.Where(r => !oldRoles.Contains(r));
            model.RolesName ??= new List<string>();
            var rolesDelete = oldRoles.Where(r => !model.RolesName.Contains(r));

            if (rolesAdd != null)
            {
                var result = await _userManager.AddToRolesAsync(user, rolesAdd);
                if (!result.Succeeded)
                {
                    foreach (var err in result.Errors)
                        ModelState.AddModelError(string.Empty, err.Description);
                }
            }

            if (rolesDelete != null)
            {
                var result = await _userManager.RemoveFromRolesAsync(user, rolesDelete);
                if (!result.Succeeded)
                {
                    foreach (var err in result.Errors)
                        ModelState.AddModelError(string.Empty, err.Description);
                }
            }

            StatusMessage = "Cập nhật Role thành công";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("{userid}")]
        public async Task<IActionResult> AddClaim(string userid)
        {
            var user = await _userManager.FindByIdAsync(userid);
            if (user == null) return NotFound("Không tìm thấy user");
            ViewBag.user = user;
            return View();
        }

        [HttpPost("{userid}"), ActionName(nameof(AddClaim))]
        public async Task<IActionResult> AddClaimAsycn(string userid, AddClaimModel model)
        {
            var user = await _userManager.FindByIdAsync(userid);
            if (user == null) return NotFound("Không tìm thấy user");
            ViewBag.user = user;
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (_context.UserClaims.Where(c => c.UserId == userid && c.ClaimType == model.ClaimType && c.ClaimValue == model.ClaimValue).Any())
            {
                ModelState.AddModelError(string.Empty, "Claim này đã tồn tại");
                return View(model);
            }

            var result = await _userManager.AddClaimAsync(user, new Claim(model.ClaimType, model.ClaimValue));
            if (result.Succeeded)
            {
                StatusMessage = "Thêm Claim thành công";
                return RedirectToAction(nameof(AddRole), new { userid = user.Id });
            }
            else
            {
                foreach (var err in result.Errors)
                    ModelState.AddModelError(string.Empty, err.Description);
            }
            return View(model);
        }

        [HttpGet("{claimid:int}")]
        public async Task<IActionResult> EditClaim(int claimid)
        {
            var claim = _context.UserClaims.Where(c => c.Id == claimid).FirstOrDefault();
            if (claim == null) return NotFound("Không tìm thấy Claim");

            var user = await _userManager.FindByIdAsync(claim.UserId);
            if (user == null) return NotFound("Không tìm thấy user");

            var model = new AddClaimModel()
            {
                ClaimType = claim.ClaimType ?? "",
                ClaimValue = claim.ClaimValue ?? ""
            };

            ViewBag.User = user;
            ViewBag.Claim = claim;

            return View(model);
        }

        [HttpPost("{claimid:int}"), ActionName(nameof(EditClaim))]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditClaimAsync(int claimid, AddClaimModel model)
        {
            var claim = _context.UserClaims.Where(c => c.Id == claimid).FirstOrDefault();
            if (claim == null) return NotFound("Không tìm thấy Claim");

            var user = await _userManager.FindByIdAsync(claim.UserId);
            if (user == null) return NotFound("Không tìm thấy user");

            ViewBag.User = user;
            ViewBag.Claim = claim;

            if (!ModelState.IsValid) return View(model);

            if (_context.UserClaims.Where(u => u.UserId == user.Id && u.ClaimType == model.ClaimType && u.ClaimValue == model.ClaimValue && u.Id != claimid).Any())
            {
                ModelState.AddModelError(string.Empty, "Claim này đã tồn tại");
                return View(model);
            }

            claim.ClaimType = model.ClaimType;
            claim.ClaimValue = model.ClaimValue;

            _context.SaveChanges();
            StatusMessage = "Cập nhật thành công";
            return RedirectToAction(nameof(AddRole), new { userid = user.Id });
        }

        [HttpGet("{userid}")]
        public async Task<IActionResult> SetPassword(string userid)
        {
            var user = await _userManager.FindByIdAsync(userid);
            if (user == null) return NotFound("Không tìm thấy user");
            ViewBag.User = user;

            return View();
        }

        [HttpPost("{userid}")]
        public async Task<IActionResult> SetPasswordAsync(string userid, SetPasswordModel model)
        {
            var user = await _userManager.FindByIdAsync(userid);
            if (user == null) return NotFound("Không tìm thấy user");
            ViewBag.User = user;

            await _userManager.RemovePasswordAsync(user);
            var result = await _userManager.AddPasswordAsync(user, model.NewPassword);
            if (result.Succeeded)
            {
                StatusMessage = $"Thiết lập mật khẩu cho user {user.UserName} thành công";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                foreach (var err in result.Errors)
                    ModelState.AddModelError(string.Empty, err.Description);
                return View(model);
            }
        }

        [HttpGet("{claimId}")]
        public async Task<IActionResult> DeleteClaim(int claimId)
        {
            var claim = _context.UserClaims.Where(c => c.Id == claimId).FirstOrDefault();
            if (claim == null) return NotFound("Không tìm thấy claim");

            var user = await _userManager.FindByIdAsync(claim.UserId);
            if (user == null) return NotFound("Không tìm thấy User");

            return View(claim);
        }

        [HttpPost("{claimId}"), ActionName(nameof(DeleteClaim))]
        public async Task<IActionResult> DeleteClaimAsync(int claimId)
        {
            var claim = _context.UserClaims.Where(c => c.Id == claimId).FirstOrDefault();
            if (claim == null) return NotFound("Không tìm thấy claim");

            var user = await _userManager.FindByIdAsync(claim.UserId);
            if (user == null) return NotFound("Không tìm thấy User");

            await _userManager.RemoveClaimAsync(user, new Claim(claim.ClaimType ?? "", claim.ClaimValue ?? ""));

            StatusMessage = "Xóa thành công Claim";
            return RedirectToAction(nameof(AddRole), new { userid = user.Id });
        }

        private void GetClaims(AddRoleUserModel model)
        {
            var listRoles = from r in _context.Roles
                            join u in _context.UserRoles on r.Id equals u.RoleId
                            where u.UserId == model.User.Id
                            select r;

            var listClaims = from c in _context.RoleClaims
                             join r in _context.Roles on c.RoleId equals r.Id
                             select c;

            model.ClaimsInRole = listClaims.ToList();

            model.ClaimsInUser = _context.UserClaims.Where(c => c.UserId == model.User.Id).ToList();
        }

        [HttpGet]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return Content("Không tìm thấy user");
            }

            return View(user);
        }

        [HttpPost, ActionName(nameof(DeleteUser))]
        public async Task<IActionResult> DeleteUserConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return Content("Không tìm thấy user");
            }

            try
            {

                var messages = await _context.Messages.Where(x => x.SenderId == id || x.ReceiverId == id).ToArrayAsync();
                _context.Messages.RemoveRange(messages);
                _context.Users.Remove(user);

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                StatusMessage = "Xóa thất bại";
                return RedirectToAction(nameof(Index));
            }

        }

    }
}