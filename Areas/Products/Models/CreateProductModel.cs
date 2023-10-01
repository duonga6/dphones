using System.ComponentModel.DataAnnotations;
using App.Models.Products;

namespace App.Areas.Products.Models
{
    public class CreateProductModel : Product
    {
        [Display(Name = "Danh mục")]
        public int[]? CategoryId {set;get;}

        public List<ColorExtend> ProductColor {set;get;} = new();

        public IFormFile? PrimaryImage {set;get;}

        public List<UploadFile> SubImage {set;get;} = new();
    }

    public class ColorExtend : Color
    {
        [FileExtensions(Extensions = "png, jpg, jpeg, webp")]
        public IFormFile? ImageFile {set;get;}
    }

    public class UploadFile
    {
        public IFormFile? FileUpload {set;get;}
    }
}