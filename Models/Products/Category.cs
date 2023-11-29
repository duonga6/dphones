using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace App.Models.Products
{
    public class Category
    {
        public int Id { set; get; }

        [Display(Name = "Tên danh mục")]
        [Required(ErrorMessage = "{0} không được trống")]
        public required string Name { set; get; }

        [Display(Name = "Mô tả")]
        [DataType(DataType.Text)]
        public string? Description { set; get; }

        [Display(Name = "Url hiện thị")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "{0} phải từ {2} đến {1} ký tự")]
        [RegularExpression(@"^[a-z0-9-]*$", ErrorMessage = "{0} chỉ chứa các ký tự a-z, 0-9, \"-\"")]
        public string? Slug { set; get; }

        // public List<Product>? Products {set;get;}
    }
}