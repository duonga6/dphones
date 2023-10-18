using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace App.Models.Products 
{
    public class PayStatus 
    {   
        [Key]
        public int Id {set;get;}
        
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Số tiền")]
        public decimal? Amount {set;get;}

        [Display(Name = "Mã ngân hàng")]
        [StringLength(20)]
        public string? BankCode {set;get;}

        [Display(Name = "Mã thanh toán ngân hàng")]
        [StringLength(255)]
        public string? BankTranNo {set;get;}

        [Display(Name = "Loại thẻ")]
        [StringLength(20)]
        public string? CardType {set;get;}

        [Display(Name = "Ngày thực hiện")]
        public DateTime? Date {set;get;}

        [Display(Name = "Mã phản hồi kết quả thanh toán")]
        [StringLength(2)]
        public string? ResponseCode {set;get;}

        [Display(Name = "Mã giao dịch VNPAY")]
        [StringLength(15)]
        public string? TransactionNo {set;get;}

        [Display(Name = "Mã phản hồi kết quả thanh toán cổng VNPAY")]
        public string? TransactionStatus {set;get;}

        [Display(Name = "Mã tham chiếu")]
        [StringLength(100)]
        public string? PaymentCode {set;get;}

        [Display(Name = "Nội dung giao dịch")]
        [StringLength(255)]
        public string? OrderInfo {set;get;}

        public string? Content {set;get;}

        public int OrderId {set;get;}
        [ForeignKey("OrderId")]
        public Order? Order {set;get;}
    }
}