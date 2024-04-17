using System.ComponentModel;

namespace App.Models.Reports;

public class OrderReport
{
    [DisplayName("Mã đơn hàng")]
    public string OrderCode { set; get; } = string.Empty;

    [DisplayName("Tên khách hàng")]
    public string Customter { set; get; } = string.Empty;

    [DisplayName("Email")]
    public string Email { set; get; } = string.Empty;

    [DisplayName("SDT")]
    public string PhoneNumber { set; get; } = string.Empty;

    [DisplayName("Ngày đặt")]
    public string OrderDate { set; get; } = string.Empty;

    [DisplayName("Tổng tiền")]
    public double Total { set; get; }

    [DisplayName("Ngày nhận")]
    public string OrderRecived { set; get; } = string.Empty;

    [DisplayName("Trạng thái")]
    public string Status { set; get; } = string.Empty;
}