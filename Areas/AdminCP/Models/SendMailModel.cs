using System.ComponentModel.DataAnnotations;

namespace App.Areas.AdminCP.Models
{
    public class SendMailModel
    {
        [Required(ErrorMessage = "{0} không được trống")]
        [Display(Name = "Tiêu đề")]
        public string Subject { set; get; } = string.Empty;

        [Required(ErrorMessage = "{0} không được trống")]
        [Display(Name = "Nội dung")]
        public string Content { set; get; } = string.Empty;

        [Required]
        public string Type { set; get; } = string.Empty;

        [Display(Name = "Người nhận")]
        public string? Receiver { set; get; }
    }
}