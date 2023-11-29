using System.ComponentModel.DataAnnotations.Schema;

namespace App.Models.Products
{
    public class ProductCategory
    {
        public int ProductId { set; get; }

        public int CategoryId { set; get; }

        [ForeignKey("ProductId")]
        public Product? Product { set; get; }

        [ForeignKey("CategoryId")]
        public Category? Category { set; get; }
    }
}