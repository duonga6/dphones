using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace App.Models.Products
{
    public class OrderDetail
    {
        [Key]
        public int Id {set;get;}
        public int OrderId {set;get;}
        public int? ProductId {set;get;}
        public int? ColorId {set;get;}
        public int? CapacityId {set;get;}
        public int Quantity {set;get;}
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal SellPrice {set;get;}

        [ForeignKey("OrderId")]
        public Order? Order {set;get;}

        [ForeignKey("ProductId")]
        public Product? Product {set;get;}
        
        [ForeignKey("ColorId")]
        public Color? Color {set;get;}
        
        [ForeignKey("CapacityId")]
        public Capacity? Capacity {set;get;}

    }
}