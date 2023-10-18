using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace App.Models.Products 
{
    public class Order 
    {   
        [Key]
        public int Id {set;get;}

        [StringLength(100)]
        public required string Code {set;get;}

        [Display(Name = "Họ tên người nhận")]
        [Required(ErrorMessage = "{0} không được trống")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "{0} phải từ {2} đến {1} ký tự")]
        public required string FullName {set;get;}

        [StringLength(50)]
        [Display(Name = "Tỉnh / Thành phố")]
        [Required(ErrorMessage = "{0} không được trống")]
        public required string City {set;get;}

        [StringLength(50)]
        [Display(Name = "Quận / Huyện")]
        [Required(ErrorMessage = "{0} không được trống")]
        public required string District {set;get;}

        [StringLength(50)]
        [Display(Name = "Phường / Xã")]
        [Required(ErrorMessage = "{0} không được trống")]
        public required string Commune {set;get;} 

        [StringLength(200, MinimumLength = 3, ErrorMessage = "{0} quá ngắn")]
        [Display(Name = "Địa chỉ cụ thể")]
        [Required(ErrorMessage = "{0} không được trống")]
        public required string SpecificAddress {set;get;}

        [Display(Name = "Số điện thoại người nhận")]
        [StringLength(10, ErrorMessage = "{0} phải có 10 chữ số")]
        [RegularExpression(@"^0\d+$", ErrorMessage = "{0} phải có dạng 0xxxxxxxxx")]
        [Required(ErrorMessage = "{0} không được trống")]
        public required string PhoneNumber {set;get;}
        
        [Display(Name = "Email")]
        [EmailAddress]
        [Required(ErrorMessage = "{0} không được trống")]
        public required string Email {set;get;}

        [Display(Name = "Ngày đặt")]
        public DateTime OrderDate {set;get;}

        [Display(Name = "Ngày nhận")]
        public DateTime? ReceivedDate {set;get;}

        [Display(Name = "Hình thức thanh toán")]
        [StringLength(50)]
        public required string PayType {set;get;}

        [Display(Name = "Thành tiền")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalCost {set;get;}

        public string? UserId {set;get;}

        [ForeignKey("UserId")]
        public AppUser? User {set;get;}

        public List<OrderDetail> OrderDetails {set;get;} = new();
        public List<OrderStatus> OrderStatuses {set;get;} = new();
        public List<PayStatus> PayStatuses {set;get;} = new();
    }
}


// Logic:
/*

    Tạo đơn hàng:   Thanh toán trực tuyến ->    Đã thanh toán: Đã xác nhận
                                          ->    Chưa thanh toán:
                    

*/