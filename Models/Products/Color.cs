using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace App.Models.Products
{
    public class Color 
    {
        [Key]
        public int Id {set;get;}

        [Required(ErrorMessage = "{0} không được trống")]
        [Display(Name = "Tên màu")]
        public string? Name {set;get;}

        [Required(ErrorMessage = "{0} không được trống")]
        [Display(Name = "Mã màu")]
        public string? Code {set;get;}

        public string? Image {set;get;}

        public int ProductId {set;get;}

        [ForeignKey("ProductId")]
        [JsonIgnore]
        public Product? Product {set;get;}

        public List<Capacity> Capacities {set;get;} = new();
    }
}