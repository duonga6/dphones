using App.Models.Products;

namespace App.Areas.Products.Models
{
    public class ProductWithRate
    {
        public Product Product {set;get;} = null!;
        public double Rate {set;get;}
    }
}