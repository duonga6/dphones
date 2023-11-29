using System.ComponentModel.DataAnnotations;

namespace App.Models.Contacts
{
    public class Contact
    {
        [Key]
        public int Id { set; get; }

        [Required(ErrorMessage = "{0} không được trống")]
        [StringLength(50)]
        [Display(Name = "Họ tên")]
        public string FullName { set; get; } = string.Empty;

        [Required(ErrorMessage = "{0} không được trống")]
        [StringLength(20)]
        [Display(Name = "Số điện thoại")]
        public string PhoneNumber { set; get; } = string.Empty;

        [Required(ErrorMessage = "{0} không được trống")]
        [EmailAddress]
        [Display(Name = "Địa chỉ Email")]
        public string Email { set; get; } = string.Empty;

        [Display(Name = "Nội dung")]
        [Required(ErrorMessage = "{0} không được trống")]
        public string Content { set; get; } = string.Empty;

        public DateTime CreatedAt { set; get; } = DateTime.Now;

        public bool Seen { set; get; }
    }
}