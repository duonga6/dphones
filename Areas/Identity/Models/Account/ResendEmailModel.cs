using System.ComponentModel.DataAnnotations;

namespace App.Areas.Identity.Models.Account
{
    public class ResendEmailModel
    {
        [Required(ErrorMessage = "Không được để trống")]
        [EmailAddress]
        [Display(Name = "Vui lòng nhập địa chỉ email")]
        public required string Email { get; set; }
    }
}