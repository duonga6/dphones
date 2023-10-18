using System.ComponentModel.DataAnnotations;

namespace App.Models.VnPay
{
    public class PayResponse
    {
        // Số tiền
        public long vnp_Amount {set;get;}

        // Mã ngân hàng
        public string? vnp_BankCode {set;get;}

        // Mã giao dịch tại ngân hàng
        public string? vnp_BankTranNo {set;get;}

        // Loại thẻ/ thanh toán
        public string? vnp_CardType {set;get;}

        // Nội dung thanh toán
        public string? vnp_OrderInfo {set;get;}

        // Ngày thực hiện
        public string vnp_PayDate {set;get;} = "00010000000000";

        // Phản hồi kết quả thanh toán
        public string? vnp_ResponseCode {set;get;}

        // Mã website
        public string? vnp_TmnCode {set;get;}

        // Mã giao dịch tại VNPAY
        public string? vnp_TransactionNo {set;get;}

        // Mã phản hồi kết quả thanh toán
        public string? vnp_TransactionStatus {set;get;}

        // Mã giao dịch thanh toán
        public string? vnp_TxnRef {set;get;}

        public string? vnp_SecureHash {set;get;}
    }
}