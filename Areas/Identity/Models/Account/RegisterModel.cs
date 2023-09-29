using System.ComponentModel.DataAnnotations;

namespace App.Areas.Identity.Models.Account
{
    public class RegisterModel
        {
            [Required(ErrorMessage = "{0} không được trống")]
            [EmailAddress]
            [Display(Name = "Email")]
            public required string Email { get; set; }

            [Required(ErrorMessage = "{0} không được trống")]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Mật khẩu")]
            public required string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Nhập lại mật khẩu")]
            [Compare("Password", ErrorMessage = "Mật khẩu không khớp.")]
            public required string ConfirmPassword { get; set; }

            [Display(Name = "Họ tên")]
            [Required(ErrorMessage = "{0} không được trống")]
            public required string FullName {set;get;}

            [Display(Name = "Tên đăng nhập")]
            [Required(ErrorMessage = "{0} không được trống")]
            public required string UserName {set;get;}
        }
}