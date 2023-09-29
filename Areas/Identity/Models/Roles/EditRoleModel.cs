using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace App.Areas.Identity.Models.Roles
{
    public class EditRoleModel
    {
        [Display(Name = "Tên Role")]
        [StringLength(255, MinimumLength = 2, ErrorMessage = "{0} phải dài từ {2} đến {1} ký tự")]
        [Required(ErrorMessage = "{0} không được để trống")]
        public required string Name {set;get;}

        public List<IdentityRoleClaim<string>>? Claims {set;get;}
        
        public string? RoleId {set;get;}
    }
}