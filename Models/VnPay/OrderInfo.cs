namespace App.Models.VnPay
{
    public class OrderInfo
    {
        public string? OrderId {set;get;}
        public long Amount {set;get;}
        public string? OrderDesc {set;get;}
        public DateTime CreatedDate {set;get;}
        public string? Status {set;get;}
        public long PaymentTranId {set;get;}
        public string? BankCode {set;get;}
        public string? PayStatus {set;get;}
    }
}