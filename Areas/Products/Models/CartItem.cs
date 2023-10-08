using App.Models.Products;

namespace App.Areas.Products.Models {
    public class CartItem 
    {
        public required string Id {set;get;}
        public required Product Product {set;get;}
        public required Color Color {set;get;}
        public required Capacity Capacity {set;get;}
        public int Quantity {set;get;}
    }
}