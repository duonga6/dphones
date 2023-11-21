using System.ComponentModel.DataAnnotations;

namespace App.Areas.Products.Models
{
    public class OrderProduct 
    {
        public int ProductId {set;get;}
        public int ColorId {set;get;}
        public int CapaId {set;get;}
        public int Quantity {set;get;}
    }
    public class OrderCreate
    {
        public string Name {set;get;} = null!;
        public string Address {set;get;} = null!;
        public string City {set;get;} = null!;
        public string District {set;get;} = null!;
        public string Commune {set;get;} = null!;
        public string Phone {set;get;} = null!;
        [EmailAddress]
        public string Email {set;get;} = null!;
        public List<OrderProduct> Products {set;get;} =new();
        public string BuyType {set;get;} = null!;
        public bool Paid {set;get;}
    }
}