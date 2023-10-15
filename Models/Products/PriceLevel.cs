using System.ComponentModel.DataAnnotations;

namespace App.Models.Products
{
    public class PriceLevel
    {   
        [Key]
        public int Id {set;get;}

        [Range(0, int.MaxValue)]
        public int Level {set;get;}
    }
}