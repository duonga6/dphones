using System.ComponentModel;
using System.Reflection;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace App.Services;

public class ExcelReportService
{
    public async Task<string> GenerateExcel<T>(string sheetName, List<T> data, string title, string? fileName = null)
    {
        var today = DateTime.Now;
        var package = new ExcelPackage();
        var sheet = package.Workbook.Worksheets.Add(sheetName);

        sheet.Cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

        sheet.Cells[1, 1].Value = title;
        sheet.Cells[1, 1].Style.Font.Bold = true;
        sheet.Cells[2, 1].Value = $"Ngày: {today.Day}/{today.Month}/{today.Year} {today.Hour}:{today.Minute}";
        sheet.Cells[2, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
        sheet.Cells[3, 1].Value = "STT";


        if (data.Count > 0)
        {
            PropertyInfo[] propertyInfos = typeof(T).GetProperties();
            sheet.Cells[1, 1, 1, propertyInfos.Length + 1].Merge = true;
            sheet.Cells[2, 1, 2, propertyInfos.Length + 1].Merge = true;

            for (int i = 0; i < propertyInfos.Length; i++)
            {
                var displayAttribute = propertyInfos[i].GetCustomAttribute<DisplayNameAttribute>();
                if (displayAttribute != null)
                {
                    sheet.Cells[3, i + 2].Value = displayAttribute.DisplayName;
                }
            }
        }

        int rowIndex = 4;
        foreach (T item in data)
        {
            sheet.Cells[rowIndex, 1].Value = rowIndex - 3;
            var properties = item?.GetType().GetProperties();
            for (int i = 0; i < properties?.Length; i++)
            {
                sheet.Cells[rowIndex, i + 2].Value = properties[i].GetValue(item);
            }
            rowIndex++;
        }

        fileName ??= $"reports_{today.ToString("yyyyMMddHHmmssfff")}.xlsx";

        string filePath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "Reports", fileName);

        sheet.Cells.AutoFitColumns();
        await package.SaveAsAsync(new FileInfo(filePath));

        return fileName;
    }
}