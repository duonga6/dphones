using System.ComponentModel.DataAnnotations.Schema;

namespace App.Models.Products
{
    public class ProductDiscount
    {
        public int ProductId { set; get; }
        [ForeignKey(nameof(ProductId))]
        public Product Product { set; get; } = null!;
        public int DiscountId { set; get; }
        [ForeignKey(nameof(DiscountId))]
        public Discount Discount { set; get; } = null!;
    }
}