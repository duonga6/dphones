using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace App.Models.Products {
    public class Capacity
    {   
        [Key]
        public int Id {set;get;}
        [Range(1, int.MaxValue)]
        public int Ram {set;get;}
        
        [Range(1, int.MaxValue)]
        public int Rom {set;get;}

        [Range(1, int.MaxValue)]
        [Display(Name = "Số lượng")]
        public int Quantity {set;get;}
        
        [Display(Name = "Đã bán")]
        public int Sold {set;get;}
        
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Giá nhập vào")]
        [Required(ErrorMessage = "{0} không được trống")]
        public decimal EntryPrice {set;get;}

        [Display(Name = "Giá bán ra")]
        [Required(ErrorMessage = "{0} không được trống")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal SellPrice {set;get;}

        public int ColorId {set;get;}

        [ForeignKey("ColorId")]
        [JsonIgnore]
        public Color? Color {set;get;}
    }
}