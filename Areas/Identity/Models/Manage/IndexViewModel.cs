using System.ComponentModel.DataAnnotations;

namespace App.Areas.Identity.Models.Manage
{
    public class IndexViewModel
    {
        [Display(Name = "User name")]
        public string? UserName { set; get; }

        [Phone]
        [Display(Name = "Phone number")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Home address")]
        public string? HomeAddress { set; get; }

        [Display(Name = "Full name")]
        public string? FullName { set; get; }

        [Display(Name = "Date of birth")]
        public DateTime BirthDate { set; get; }

        public string? UserAvatar { set; get; }

        public IFormFile? ImageFile { set; get; }
    }
}