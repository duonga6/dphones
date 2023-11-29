using System.ComponentModel.DataAnnotations;

namespace App.Areas.Products.Models
{
    public class CreateDiscountModel
    {
        [Display(Name = "Phần trăm giảm")]
        [Required(ErrorMessage = "{0} không được trống")]
        [Range(0, 100)]
        public int PercenDiscount { set; get; }

        [Display(Name = "Số tiền giảm")]
        [Required(ErrorMessage = "{0} không được trống")]
        [Range(0, int.MaxValue)]
        public decimal MoneyDiscount { set; get; }

        [Display(Name = "Ngày bắt đầu")]
        [Required(ErrorMessage = "{0} không được trống")]
        public DateTime StartAt { set; get; }

        [Display(Name = "Ngày kết thúc")]
        [Required(ErrorMessage = "{0} không được trống")]
        public DateTime EndAt { set; get; }

        [Display(Name = "Nội dung")]
        [Required(ErrorMessage = "{0} không được trống")]
        [StringLength(255)]
        public string Content { set; get; } = string.Empty;

        [Display(Name = "Sản phẩm áp dụng")]
        public List<int>? ProductIds { set; get; }
    }
}