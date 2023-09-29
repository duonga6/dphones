using System.ComponentModel.DataAnnotations;

namespace App.Areas.Identity.Models.Account
{
    public class ForgotPasswordModel
    {   
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public required string Email {set;get;}
    }
}