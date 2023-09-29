namespace App.Models
{
    public class HomeProductModel
    {
        public int Id {set;get;}
        public required string Name {set;get;}
        public decimal SellingPrice {set;get;}
        public string? Image {set;get;}
        public List<string?>? ProductInfo {set;get;}
    }
}