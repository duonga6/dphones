using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace App.Models.Products
{
    public class Brand
    {
        [Key]
        public int Id { set; get; }

        [Required(ErrorMessage = "{0} không được trống")]
        [Display(Name = "Tên hãng")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "{0} phải từ {2} đến {1} ký tự!")]
        public required string Name { set; get; }

        [Display(Name = "Mô tả")]
        [DataType(DataType.Text)]
        public string? Description { set; get; }

        [Display(Name = "Url hiện thị")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "{0} phải từ {2} đến {1} ký tự")]
        [RegularExpression(@"^[a-z0-9-]*$", ErrorMessage = "{0} chỉ chứa các ký tự a-z, 0-9, \"-\"")]
        public string? Slug { set; get; }

        public List<Product>? Products { set; get; }
    }
}