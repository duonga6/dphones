using System.Security.Claims;
using App.Areas.Identity.Models.Roles;
using App.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Differencing;

namespace App.Areas.Identity.Controllers
{
    [Area("Identity")]
    [Route("/Roles/[action]")]
    public class RolesController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        private readonly AppDbContext _context;

        [TempData]
        public string? StatusMessage { set; get; }

        public RolesController(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, AppDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var roles = _roleManager.Roles.OrderBy(r => r.Name).ToList();
            var rolesList = new List<RoleModel>();
            foreach (var item in roles)
            {
                var claims = await _roleManager.GetClaimsAsync(item);
                var claimList = claims.Select(c => c).ToList();
                rolesList.Add(new RoleModel()
                {
                    Id = item.Id,
                    Name = item.Name,
                    Claims = claimList
                });
            }

            return View(rolesList);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost, ActionName(nameof(Create))]
        public async Task<IActionResult> CreateAsync(AddRoleModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            var newRole = new IdentityRole(model.Name);
            var result = await _roleManager.CreateAsync(newRole);

            if (result.Succeeded)
            {
                StatusMessage = "Thêm Role thành công";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                foreach (var err in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, err.Description);
                }
                return View();
            }
        }

        [HttpGet("{roleid}")]
        public async Task<IActionResult> Edit(string roleid, [Bind("Name")] EditRoleModel model)
        {
            var role = await _roleManager.FindByIdAsync(roleid);
            if (role is null)
            {
                return NotFound("Không tìm thấy Role");
            }

            model.Name = role.Name ?? "";
            model.RoleId = role.Id;
            model.Claims = _context.RoleClaims.Where(r => r.RoleId == roleid).ToList();


            return View(model);
        }

        [HttpPost("{roleid}"), ActionName(nameof(Edit))]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAsync(string roleid, EditRoleModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var role = await _roleManager.FindByIdAsync(roleid);
            if (role is null) return NotFound("Không tìm thấy role");

            role.Name = model.Name;
            var result = await _roleManager.UpdateAsync(role);

            if (result.Succeeded)
            {
                StatusMessage = "Cập nhật Role thành công";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                foreach (var err in result.Errors)
                    ModelState.AddModelError(string.Empty, err.Description);
                return View(model);
            }
        }

        [HttpGet("{roleid}")]
        public async Task<IActionResult> Delete(string roleid)
        {
            var role = await _roleManager.FindByIdAsync(roleid);
            if (role is null) return NotFound("Không tìm thấy Role");
            return View(role);
        }

        [HttpPost("{roleid}"), ActionName(nameof(Delete))]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAsync(string roleid)
        {
            var role = await _roleManager.FindByIdAsync(roleid);
            if (role is null) return NotFound("Không tìm thấy Role");

            var result = await _roleManager.DeleteAsync(role);

            StatusMessage = result.Succeeded ? $"Xóa Role {role.Name} thành công" : $"Xóa Role {role.Name} thất bại";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet("{roleid}")]
        public async Task<IActionResult> AddRoleClaim(string roleid)
        {
            var role = await _roleManager.FindByIdAsync(roleid);
            if (role is null) return NotFound("Không tìm thấy Role");

            ViewBag.Role = role;

            return View();
        }

        [HttpPost("{roleid}"), ActionName(nameof(AddRoleClaim))]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRoleClaimAsync(string roleid, [Bind("ClaimType", "ClaimValue")] EditClaimModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var role = await _roleManager.FindByIdAsync(roleid);
            if (role is null) return NotFound("Không tìm thấy Role");

            model.Role = role;

            if (_context.RoleClaims.Where(r => r.RoleId == roleid && r.ClaimType == model.ClaimType && r.ClaimValue == model.ClaimValue).Any())
            {
                ModelState.AddModelError(string.Empty, "Thêm Claim thất bại, Claim này đã tồn tại");
                return View(model);
            }


            var result = await _roleManager.AddClaimAsync(role, new Claim(model.ClaimType, model.ClaimValue));

            if (result.Succeeded)
            {
                StatusMessage = "Thêm Claim thành công";
                return RedirectToAction(nameof(Edit), new { roleid });
            }
            else
            {
                StatusMessage = "Thêm Claim thất bại";
                foreach (var err in result.Errors)
                    ModelState.AddModelError(string.Empty, err.Description);
            }

            return View(model);
        }

        [HttpGet("{claimid:int}")]
        public async Task<IActionResult> EditClaim(int claimid)
        {
            var claim = _context.RoleClaims.Where(r => r.Id == claimid).FirstOrDefault();
            if (claim is null) return NotFound("Không tìm thấy claim");
            var role = await _roleManager.FindByIdAsync(claim.RoleId);
            if (role is null) return NotFound("Không tìm thấy Role của Claim này");

            var model = new EditClaimModel
            {
                ClaimType = claim.ClaimType ?? "",
                ClaimValue = claim.ClaimValue ?? "",
                Role = role
            };

            return View(model);
        }

        [HttpPost("{claimid:int}"), ActionName(nameof(EditClaim))]
        public async Task<IActionResult> EditClaimAsync(int claimid, EditClaimModel model)
        {
            var claim = _context.RoleClaims.Where(r => r.Id == claimid).FirstOrDefault();
            if (claim is null) return NotFound("Không tìm thấy claim");
            var role = await _roleManager.FindByIdAsync(claim.RoleId);
            if (role is null) return NotFound("Không tìm thấy Role của Claim này");

            if (_context.RoleClaims.Where(r => r.RoleId == role.Id && r.ClaimType == model.ClaimType && r.ClaimValue == model.ClaimValue && r.Id != claim.Id).Any())
            {
                ModelState.AddModelError(string.Empty, "Claim này đã tồn tại");
                return View(model);
            }

            claim.ClaimType = model.ClaimType;
            claim.ClaimValue = model.ClaimValue;

            await _context.SaveChangesAsync();
            StatusMessage = "Cập nhật Claim thành công";
            return RedirectToAction(nameof(Edit), new { role.Id });
        }

        [HttpGet("{claimid:int}")]
        public async Task<IActionResult> DeleteClaim(int claimid)
        {
            var claim = _context.RoleClaims.Where(r => r.Id == claimid).FirstOrDefault();
            if (claim is null) return NotFound("Không tìm thấy claim");
            var role = await _roleManager.FindByIdAsync(claim.RoleId);
            if (role is null) return NotFound("Không tìm thấy Role của Claim này");

            var model = new EditClaimModel
            {
                ClaimType = claim.ClaimType ?? "",
                ClaimValue = claim.ClaimValue ?? "",
                Role = role,
                Id = claimid
            };

            return View(model);
        }

        [HttpPost("{claimid:int}"), ActionName(nameof(DeleteClaim))]
        public async Task<IActionResult> DeleteClaimAsync(int claimid)
        {
            var claim = _context.RoleClaims.Where(r => r.Id == claimid).FirstOrDefault();
            if (claim is null) return NotFound("Không tìm thấy claim");
            var role = await _roleManager.FindByIdAsync(claim.RoleId);
            if (role is null) return NotFound("Không tìm thấy Role của Claim này");

            await _roleManager.RemoveClaimAsync(role, new Claim(claim.ClaimType ?? "", claim.ClaimValue ?? ""));

            StatusMessage = $"Xóa thành công Claim {claim.ClaimType} : {claim.ClaimValue}";

            return RedirectToAction(nameof(Edit), new { roleid = role.Id });
        }
    }
}