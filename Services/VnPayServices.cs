using App.Models.VnPay;
using App.Utilities;
using Microsoft.Extensions.Options;

namespace App.Services
{
    public class VnPaySettings {
        public required string Vnp_Returnurl {set;get;}
        public required string Vnp_Url {set;get;}
        public required string Vnp_TmnCode {set;get;}
        public required string Vnp_HashSecret {set;get;}
    }
    public class VnPayService
    {
        private readonly VnPaySettings _vnPaySettings;
        private readonly ILogger<VnPayService> _logger;
        private readonly HttpContext _httpContext;
        private readonly IWebHostEnvironment _environment;

        public VnPayService(ILogger<VnPayService> logger, IOptions<VnPaySettings> vnPaySettings, IHttpContextAccessor httpContextAccessor, IWebHostEnvironment environment)
        {
            _logger = logger;
            _vnPaySettings = vnPaySettings.Value;
            _httpContext = httpContextAccessor.HttpContext ?? throw new Exception("HttpContext is not avalible VnPayService");
            _environment = environment;
        }

        public string SendRequest(long amount, string orderCode)
        {
            string vnp_Returnurl = _vnPaySettings.Vnp_Returnurl; //URL nhan ket qua tra ve (Url của website bán hàng trả về kết quả cho người dùng)
            if (_environment.IsDevelopment())
            {
                vnp_Returnurl = "http://localhost:8090/ket-qua-thanh-toan";
            }
            string vnp_Url = _vnPaySettings.Vnp_Url; //URL thanh toan cua VNPAY 
            string vnp_TmnCode = _vnPaySettings.Vnp_TmnCode; //Ma định danh merchant kết nối (Terminal Id) (Lấy trên Mail đăng ký test Vnpay)
            string vnp_HashSecret = _vnPaySettings.Vnp_HashSecret; //Secret Key (Lấy trên Mail đăng ký test Vnpay)

            OrderInfo order = new()
            {
                OrderId = DateTime.Now.Ticks.ToString(), // Giả lập mã giao dịch hệ thống merchant gửi sang VNPAY
                Amount = amount, // Giả lập số tiền thanh toán hệ thống merchant gửi sang VNPAY 100,000 VND
                Status = "0", //0: Trạng thái thanh toán "chờ thanh toán" hoặc "Pending" khởi tạo giao dịch chưa có IPN
                CreatedDate = DateTime.Now
            };

            VnPayLibrary vnpay = new();
            vnpay.AddRequestData("vnp_Version", VnPayLibrary.VERSION);
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
            vnpay.AddRequestData("vnp_Amount", (order.Amount * 100).ToString()); //Số tiền thanh toán. Số tiền không mang các ký tự phân tách thập phân, phần nghìn, ký tự tiền tệ. Để gửi số tiền thanh toán là 100,000 VND (một trăm nghìn VNĐ) thì merchant cần nhân thêm 100 lần (khử phần thập phân), sau đó gửi sang VNPAY là: 10000000
            
            vnpay.AddRequestData("vnp_CreateDate", order.CreatedDate.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", Utils.GetIpAddress(_httpContext));
            vnpay.AddRequestData("vnp_Locale", "vn");

            vnpay.AddRequestData("vnp_OrderInfo", $"Thanh toan don hang DPHONES. Ma don hang: {orderCode}. Ma thanh toan: " + order.OrderId);
            vnpay.AddRequestData("vnp_OrderType", "110000"); //default value: other

            vnpay.AddRequestData("vnp_ReturnUrl", vnp_Returnurl);
            vnpay.AddRequestData("vnp_TxnRef", order.OrderId); // Mã tham chiếu của giao dịch tại hệ thống của merchant. Mã này là duy nhất dùng để phân biệt các đơn hàng gửi sang VNPAY. Không được trùng lặp trong ngày

            string paymentUrl = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);
            // _logger.LogInformation($"VNPAY URL: {paymentUrl}");
            return paymentUrl;
        }
    }
}