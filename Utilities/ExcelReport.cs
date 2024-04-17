using App.Models.Products;
using OfficeOpenXml;

namespace App.Utilities;

public static class ExcelReport
{
    public static void GenerateReportProduct(List<Product> products)
    {
        var today = DateTime.Now;
        string fileName = $"report-{today.ToString("yyyyMMddHHmmssfff")}.xlsx";
        string path = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "Reports", fileName);

        var package = new ExcelPackage(path);
        var sheet = package.Workbook.Worksheets.Add("Data");

        sheet.Cells[1, 1].Value = "Thống kê sản phẩm của shop";
        sheet.Cells[2, 1].Value = $"Ngày: {today.Day}/{today.Month}/{today.Year} ${today.Hour}:${today.Minute}";

        string[] header = new string[] { "STT", "Mã SP", "Tên SP", "Hãng", "Màu sắc", "Ram", "Rom", "Giá nhập", "Giá bán", "Số lượng kho", "Đã bán", "Trạng thái" };
        for (int i = 0; i < header.Length; i++)
        {
            sheet.Cells[3, i + 1].Value = header[i];
        }

        int rowIndex = 4;

        products.ForEach(p =>
        {
            p.Colors.ForEach(cl =>
            {
                cl.Capacities.ForEach(capa =>
                {
                    sheet.Cells[rowIndex, 1].Value = rowIndex - 3;
                    sheet.Cells[rowIndex, 2].Value = p.Code;
                    sheet.Cells[rowIndex, 3].Value = p.Name;
                    sheet.Cells[rowIndex, 4].Value = p.Brand?.Name;
                    sheet.Cells[rowIndex, 5].Value = cl.Name;
                    sheet.Cells[rowIndex, 6].Value = capa.Ram;
                    sheet.Cells[rowIndex, 7].Value = capa.Rom;
                    sheet.Cells[rowIndex, 8].Value = capa.EntryPrice;
                    sheet.Cells[rowIndex, 9].Value = capa.SellPrice;
                    sheet.Cells[rowIndex, 10].Value = capa.Quantity;
                    sheet.Cells[rowIndex, 11].Value = capa.Sold;
                    sheet.Cells[rowIndex, 12].Value = p.Published;
                    rowIndex++;
                });
            });
        });


    }
}