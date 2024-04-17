using System.ComponentModel;

namespace App.Models.Reports;

public class ProductReport
{
    [DisplayName("Mã SP")]
    public string? Code { set; get; }

    [DisplayName("Tên SP")]
    public string Name { set; get; } = string.Empty;

    [DisplayName("Hãng SX")]
    public string Brand { set; get; } = string.Empty;

    [DisplayName("Màu sắc")]
    public string Color { set; get; } = string.Empty;

    [DisplayName("Ram")]
    public int Ram { set; get; }

    [DisplayName("Rom")]
    public int Rom { set; get; }

    [DisplayName("Giá nhập")]
    public double EntryPrice { set; get; }

    [DisplayName("Giá bán")]
    public double SellPrice { set; get; }

    [DisplayName("Ngày nhập kho")]
    public string EntryDate { set; get; } = string.Empty;

    [DisplayName("SL kho")]
    public int Quantity { set; get; }

    [DisplayName("Đã bán")]
    public int Sold { set; get; }

    [DisplayName("Tình trạng")]
    public string SellStatus { set; get; } = string.Empty;

    [DisplayName("Trạng thái")]
    public string Status { set; get; } = string.Empty;
}