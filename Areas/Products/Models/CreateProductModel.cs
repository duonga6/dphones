using System.ComponentModel.DataAnnotations;
using App.Models.Products;
using App.Utilities;

namespace App.Areas.Products.Models
{
    public class CreateProductModel : Product
    {
        [Display(Name = "Danh mục")]
        public int[]? CategoryId {set;get;}

        public List<ColorExtend>? ProductColor {set;get;}

        public List<ProductPhotoWithFile>? SubImage {set;get;}
    }

    public class ColorExtend : Color
    {
        [AllowedExtensions(new string[] { ".jpg", ".png", ".webp", "jpeg" })]
        public IFormFile? ImageFile {set;get;}
    }

    public class ProductPhotoWithFile : ProductPhoto
    {
        [AllowedExtensions(new string[] { ".jpg", ".png", ".webp", "jpeg" })]
        public IFormFile? FileUpload {set;get;}

        public ProductPhotoWithFile() {
            Name = "";
        }
    }
}


/*

    Gửi đến
    + Có ảnh: file != null
        -Thêm: id == null
        -Cập nhật: id != null

    + Không có ảnh: file == null
        - Xóa: id == null
        - Giữ nguyên: id != null

*/