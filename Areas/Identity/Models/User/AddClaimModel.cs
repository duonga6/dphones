using System.ComponentModel.DataAnnotations;

namespace App.Areas.Identity.Models.User
{
    public class AddClaimModel
    {
        [Required(ErrorMessage = "{0} không được trống")]
        [Display(Name = "Kiểu Claim")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "{0} phải từ {2} đến {1} ký tự")]
        public required string ClaimType {set;get;}

        [Required(ErrorMessage = "{0} không được trống")]
        [Display(Name = "Giá trị")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "{0} phải từ {2} đến {1} ký tự")]
        public required string ClaimValue {set;get;}
    }
}