using System.ComponentModel.DataAnnotations;
using App.Models.Products;

namespace App.Areas.Products.Models
{
    public class CreateProductModel : Product
    {
        [Display(Name = "Danh mục")]
        public int[]? CategoryId {set;get;}
    }
}