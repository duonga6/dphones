using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        public int ColorId {set;get;}

        [ForeignKey("ColorId")]
        public required Color Color {set;get;}
    }
}