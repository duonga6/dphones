using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace App.Models.Products
{
    public class Product
    {
        // Property
        [Key]
        public int Id { set; get; }

        [Display(Name = "Mã sản phẩm")]
        [StringLength(20, MinimumLength = 5, ErrorMessage = "{0} phải từ {2} đến {1} ký tự")]
        public string? Code { set; get; }

        [Display(Name = "Tên sản phẩm")]
        [Required(ErrorMessage = "{0} không được trống")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "{0} phải từ {2} đến {1} ký tự")]
        public required string Name { set; get; }

        [Display(Name = "Url hiển thị")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "{0} phải từ {2} đến {1} ký tự")]
        [RegularExpression(@"^[a-z0-9-]*$", ErrorMessage = "{0} chỉ chứa các ký tự a-z, 0-9, \"-\"")]
        public string? Slug { set; get; }

        [Display(Name = "Kích thước màn hình")]
        [Range(0, 20, ErrorMessage = "{0} không hợp lệ")]
        public double? ScreenSize { set; get; }

        [Display(Name = "Thông số camera")]
        public string? Camera { set; get; }

        [Display(Name = "Vi xử lý")]
        public string? Chipset { set; get; }

        [Display(Name = "Dung lượng pin")]
        [Range(0, int.MaxValue, ErrorMessage = "{0} không hợp lệ")]
        public int? Battery { set; get; }

        [Display(Name = "Công suất sạc")]
        [Range(0, int.MaxValue, ErrorMessage = "{0} không hợp lệ")]
        public double? Charger { set; get; }

        [Display(Name = "SIM")]
        public string? SIM { set; get; }

        [Display(Name = "Hệ điều hành")]
        public string? OS { set; get; }

        [Display(Name = "Mô tả")]
        public string? Description { set; get; }

        [Display(Name = "Hiển thị sản phẩm")]
        public bool Published { set; get; }

        [Display(Name = "Ngày nhập kho")]
        public DateTime EntryDate { set; get; }

        [Display(Name = "Ngày ra mắt")]
        public DateTime? ReleaseDate { set; get; }

        // Photo
        public List<ProductPhoto>? Photos { set; get; }

        // Brand
        [Display(Name = "Hãng sản phẩm")]
        public int? BrandId { set; get; }

        [ForeignKey("BrandId")]
        [Display(Name = "Hãng")]
        public Brand? Brand { set; get; }

        // Category
        public List<ProductCategory> ProductCategories { set; get; } = new();

        public List<Color> Colors { set; get; } = new();

        public List<Review> Reviews { set; get; } = new();

        public List<ProductDiscount> ProductDiscounts { set; get; } = new();
    }
}