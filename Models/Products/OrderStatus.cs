using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace App.Models.Products
{
    public class OrderStatus
    {   
        [Key]
        public int Id {set;get;}

        [Display(Name = "Mã trạng thái")]
        [Required]
        public int Code {set;get;}

        [Display(Name = "Trạng thái")]
        [Required]
        public string Status {set;get;} = "";

        [Display(Name = "Ngày cập nhật")]
        public DateTime DateUpdate {set;get;}
        
        [Display(Name = "Ghi chú")]
        public string? Note {set;get;}
        
        public int OrderId {set;get;}
        [ForeignKey("OrderId")]
        public Order? Order {set;get;}

        public string? UserId {set;get;}

        [ForeignKey("UserId")]
        public AppUser? User {set;get;}

    }
}