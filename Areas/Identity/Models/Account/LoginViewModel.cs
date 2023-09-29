using System.ComponentModel.DataAnnotations;

namespace App.Areas.Identity.Models.Account
{
    public class LoginViewModel
        {
            [Required(ErrorMessage = "{0} không được trống")]
            [Display(Name = "Tên đăng nhập / email")]
            public required string Email { get; set; }

            [Required(ErrorMessage = "{0} không được trống")]
            [Display(Name = "Mật khẩu")]
            [DataType(DataType.Password)]
            public required string Password { get; set; }

            [Display(Name = "Ghi nhớ đăng nhập")]
            public bool RememberMe { get; set; }
        }
}