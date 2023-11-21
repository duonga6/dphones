namespace App.Areas.Products.Models.Cart
{
    public class AddToCartRequest
    {
        public int productId { set; get; }
        public int colorId { set; get; }
        public int capaId { set; get; }
    }
}