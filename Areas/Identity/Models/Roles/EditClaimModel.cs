using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace App.Areas.Identity.Models.Roles
{
    public class EditClaimModel
    {
        public int Id {set;get;}
        [Required(ErrorMessage = "{0} không được trống")]
        [Display(Name = "Kiểu Claim")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "{0} phải từ {2} đến {1} ký tự")]
        public required string ClaimType {set;get;}

        [Required(ErrorMessage = "{0} không được trống")]
        [Display(Name = "Giá trị")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "{0} phải từ {2} đến {1} ký tự")]
        public required string ClaimValue {set;get;}

        public IdentityRole? Role {set;get;}
    }
}