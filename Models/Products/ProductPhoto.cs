using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace App.Models.Products
{
    public class ProductPhoto
    {
        [Key]
        public int Id { set; get; }

        public required string Name { set; get; }

        public int ProductId { set; get; }

        [ForeignKey("ProductId")]
        public Product? Product { set; get; }
    }
}