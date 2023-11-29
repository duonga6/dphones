using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace App.Models.Products
{
    public class Discount
    {
        [Key]
        public int Id { set; get; }

        [Display(Name = "Phần trăm giảm")]
        [Required(ErrorMessage = "{0} không được trống")]
        public int PercentDiscount { set; get; }

        [Display(Name = "Số tiền giảm")]
        [Required(ErrorMessage = "{0} không được trống")]
        [Column(TypeName = "decimal(18,2)")]
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

        public List<ProductDiscount> ProductDiscounts { set; get; } = new();

    }
}