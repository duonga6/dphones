using System.ComponentModel.DataAnnotations;

namespace App.Areas.Identity.Models.User
{
    public class SetPasswordModel
    {
        [Required(ErrorMessage = "{0} không được trống")]
        [StringLength(100, ErrorMessage = "{0} phải từ {2} đến {1} ký tự.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu mới")]
        public required string NewPassword { get; set; }

        [Required(ErrorMessage = "{0} không được trống")]
        [DataType(DataType.Password)]
        [Display(Name = "Nhập lại mật khẩu mới")]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu không trùng khớp.")]
        public required string ConfirmPassword { get; set; }
    }
}