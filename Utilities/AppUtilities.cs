using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using App.Models.Products;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace App.Utilities
{
    public class AppUtilities
    {
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Normalize the domain
                email = Regex.Replace(email, @"(@)(.+)$", DomainMapper,
                                      RegexOptions.None, TimeSpan.FromMilliseconds(200));

                // Examines the domain part of the email and normalizes it.
                string DomainMapper(Match match)
                {
                    // Use IdnMapping class to convert Unicode domain names.
                    var idn = new IdnMapping();

                    // Pull out and process domain name (throws ArgumentException on invalid)
                    string domainName = idn.GetAscii(match.Groups[2].Value);

                    return match.Groups[1].Value + domainName;
                }
            }
            catch
            {
                return false;
            }

            try
            {
                return Regex.IsMatch(email,
                    @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                    RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        public static string GenerateSlug(string str, bool hierarchical = true)
        {
            var slug = str.Trim().ToLower();

            string[] decomposed = new string[] { "à","á","ạ","ả","ã","â","ầ","ấ","ậ","ẩ","ẫ","ă",
                                                    "ằ","ắ","ặ","ẳ","ẵ","è","é","ẹ","ẻ","ẽ","ê","ề" ,
                                                    "ế","ệ","ể","ễ", "ì","í","ị","ỉ","ĩ", "ò","ó","ọ",
                                                    "ỏ","õ","ô","ồ","ố","ộ","ổ","ỗ","ơ" ,"ò","ớ","ợ","ở",
                                                    "õ", "ù","ú","ụ","ủ","ũ","ư","ừ","ứ","ự","ử","ữ",
                                                    "ỳ","ý","ỵ","ỷ","ỹ", "đ",
                                                    "À","À","Ạ","Ả","Ã","Â","Ầ","Ấ","Ậ","Ẩ","Ẫ","Ă" ,
                                                    "Ằ","Ắ","Ặ","Ẳ","Ẵ", "È","É","Ẹ","Ẻ","Ẽ","Ê","Ề",
                                                    "Ế","Ệ","Ể","Ễ", "Ì","Í","Ị","Ỉ","Ĩ", "Ò","Ó","Ọ","Ỏ",
                                                    "Õ","Ô","Ồ","Ố","Ộ","Ổ","Ỗ","Ơ" ,"Ờ","Ớ","Ợ","Ở","Ỡ",
                                                    "Ù","Ú","Ụ","Ủ","Ũ","Ư","Ừ","Ứ","Ự","Ử","Ữ", "Ỳ","Ý","Ỵ",
                                                    "Ỷ","Ỹ", "Đ"};
            string[] precomposed =  {  "à","á","ạ","ả","ã","â","ầ","ấ","ậ","ẩ","ẫ","ă",
                                        "ằ","ắ","ặ","ẳ","ẵ","è","é","ẹ","ẻ","ẽ","ê","ề" ,
                                        "ế","ệ","ể","ễ", "ì","í","ị","ỉ","ĩ", "ò","ó","ọ","ỏ",
                                        "õ","ô","ồ","ố","ộ","ổ","ỗ","ơ" ,"ờ","ớ","ợ","ở","ỡ", "ù",
                                        "ú","ụ","ủ","ũ","ư","ừ","ứ","ự","ử","ữ", "ỳ","ý","ỵ","ỷ","ỹ",
                                        "đ", "À","Á","Ạ","Ả","Ã","Â","Ầ","Ấ","Ậ","Ẩ","Ẫ","Ă" ,"Ằ","Ắ",
                                        "Ặ","Ẳ","Ẵ", "È","É","Ẹ","Ẻ","Ẽ","Ê","Ề","Ế","Ệ","Ể","Ễ", "Ì",
                                        "Í","Ị","Ỉ","Ĩ", "Ò","Ó","Ọ","Ỏ","Õ","Ô","Ồ","Ố","Ộ","Ổ","Ỗ",
                                        "Ơ" ,"Ờ","Ớ","Ợ","Ở","Ỡ", "Ù","Ú","Ụ","Ủ","Ũ","Ư","Ừ","Ứ","Ự",
                                        "Ử","Ữ", "Ỳ","Ý","Ỵ","Ỷ","Ỹ", "Đ"};
            string[] latin =  { "a","a","a","a","a","a","a","a","a","a","a" ,
                                "a","a","a","a","a","a", "e","e","e","e","e",
                                "e","e","e","e","e","e", "i","i","i","i","i", "o",
                                "o","o","o","o","o","o","o","o","o","o","o" ,"o","o","o","o","o",
                                "u","u","u","u","u","u","u","u","u","u","u", "y","y","y","y","y", "d",
                                "a","a","a","a","a","a","a","a","a","a","a","a" ,"a","a","a","a","a",
                                "e","e","e","e","e","e","e","e","e","e","e", "i","i","i","i","i", "o",
                                "o","o","o","o","o","o","o","o","o","o","o" ,"o","o","o","o","o", "u",
                                "u","u","u","u","u","u","u","u","u","u", "y","y","y","y","y", "d"};

            // Convert culture specific characters
            for (int i = 0; i < decomposed.Length; i++)
            {
                slug = slug.Replace(decomposed[i], latin[i]);
                slug = slug.Replace(precomposed[i], latin[i]);
            }

            // Remove special characters
            slug = Regex.Replace(slug, @"[^a-z0-9-/ ]", "").Replace("--", "-");

            // Remove whitespaces
            slug = Regex.Replace(slug.Replace("-", " "), @"\s+", " ").Replace(" ", "-");

            // Remove slash if non-hierarchical
            if (!hierarchical)
                slug = slug.Replace("/", "-");

            // Remove multiple dashes
            slug = Regex.Replace(slug, @"[-]+", "-");

            // Remove leading & trailing dashes
            if (slug.EndsWith("-"))
                slug = slug.Substring(0, slug.LastIndexOf("-"));
            if (slug.StartsWith("-"))
                slug = slug.Substring(Math.Min(slug.IndexOf("-") + 1, slug.Length));
            return slug;
        }

        public static string GenerateHtmlEmail(string userName, string content)
        {
            string html = $@"
                <table width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" style=""background-color: #F6F8FC; padding-top: 20px; padding-bottom: 40px;"">
                    <link href='https://fonts.googleapis.com/css?family=Roboto' rel='stylesheet'>
                    <tbody>
                        <tr>
                            <td align=""center"" valign=""center"">
                                <table style=""border-collapse: collapse; width: 600px; font-family: 'Roboto'"">
                                    <tbody>
                                        <tr>
                                            <td>
                                                <div>
                                                    <img src=""https://i.postimg.cc/43f45DSk/logo-color.png"" alt=""""
                                                        style=""width: 240px; object-fit: contain;"">
                                                </div>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td style=""background-color: white; border-radius: 8px; overflow: hidden;"">
                                                <div
                                                    style=""height: 6px; width: 100%;background-image: url('https://i.postimg.cc/DfjBHp12/phong-thu.jpg'); background-repeat: repeat-x; background-size: contain;"">
                                                </div>
                                                <div
                                                    style=""padding: 36px; height: 400px; color: #7c8088;"">
                                                    <p style=""font-family: 'GOOGLE SANS';margin: 14px 0; font-size: 14px; color: #7c8088;"">Xin chào {userName},</p>
                                                    <pre style=""font-family: 'GOOGLE SANS';height: 336px; color: #7c8088; font-size: 14px;"">{content}</pre>
                                                    <p style=""font-size: 14px;font-family: 'GOOGLE SANS';"">DPhoneS - {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}</p>
                                                </div>
                                                <div
                                                    style=""height: 6px; width: 100%;background-image: url('https://i.postimg.cc/DfjBHp12/phong-thu.jpg'); background-repeat: repeat-x; background-size: contain;"">
                                                </div>
                                            </td>
                                        </tr>
                                    </tbody>
                                </table>
                            </td>
                        </tr>
                    </tbody>
                </table>
            ";

            return html;
        }


        public static string GenerateHtmlEmailVeryfiAccount(string username, string callbackUrl)
        {
            string emailContent =
$@"
Hãy xác thực tài khoản của bạn bằng cách <a href='{callbackUrl}'>ấn vào đây</a>.
";
            return GenerateHtmlEmail(username, emailContent);
        }

        public static async Task<string> GetServerIpAddress()
        {
            string url = "http://ipinfo.io/ip";
            using (HttpClient client = new())
            {
                try
                {
                    string ipAddress = await client.GetStringAsync(url);
                    return ipAddress;
                }
                catch
                {
                    return "127.0.0.1";
                }
            }
        }

        public static string GenerateBill(Order order)
        {
            if (order == null) return "error";
            try
            {
                PdfDocument document = new();

                PdfPage page = document.AddPage();
                page.Size = PdfSharpCore.PageSize.RA4;

                XGraphics gfx = XGraphics.FromPdfPage(page);

                XFont fontsm = new XFont("Arial", 12, XFontStyle.Regular);
                XFont font = new XFont("Arial", 14, XFontStyle.Regular);
                XFont fontItalic = new XFont("Arial", 14, XFontStyle.Italic);
                XFont fontBig = new XFont("Arial", 22, XFontStyle.Bold);

                gfx.DrawString(
                "Số 298 Đ. Cầu Diễn, Minh Khai, Bắc Từ Liêm, Hà Nội", font, XBrushes.Black,
                new XRect(16, 20, page.Width - 40, 14),
                XStringFormats.TopRight);

                gfx.DrawString(
                "Điện thoại: 1800.1789 - 0123456678", font, XBrushes.Black,
                new XRect(16, 20 + 18, page.Width - 40, 14),
                XStringFormats.TopRight);

                gfx.DrawString(
                    "Email: shopdienthoai@gmail.com", font, XBrushes.Black,
                    new XRect(16, 20 + 18 * 2, page.Width - 40, 14),
                    XStringFormats.TopRight);

                XImage image = XImage.FromFile("wwwroot\\images\\logo-color.png");
                gfx.DrawImage(image, 20, 23, 200, 50);

                gfx.DrawString(
                "HÓA ĐƠN BÁN HÀNG", fontBig, XBrushes.Black,
                new XRect(0, 90, page.Width, 24),
                XStringFormats.Center);

                gfx.DrawString(
                "Tên khách hàng: Dương Phạm", font, XBrushes.Black,
                new XRect(20, 140, page.Width / 2, 14),
                XStringFormats.TopLeft);

                gfx.DrawString(
                "Số điện thoại: 0123456789", font, XBrushes.Black,
                new XRect(page.Width / 2, 140, page.Width / 2, 14),
                XStringFormats.TopLeft);

                gfx.DrawString(
                "Địa chỉ: SN 6, Ngõ 5, TDP 3, Phường Phúc Xá, Quận Ba Đình, Thành phố Hà Nội", font, XBrushes.Black,
                new XRect(20, 140 + 20, page.Width - 40, 30),
                XStringFormats.TopLeft);

                gfx.DrawString(
                    "Email: abc@gmail.com", font, XBrushes.Black,
                    new XRect(20, 140 + 20 * 2, page.Width - 40, 30),
                    XStringFormats.TopLeft);

                var rowHeight = 28;
                var tableYStart = 190;

                var col1Offset = 20;
                var col2Offset = 60;
                var col3Offset = 280;
                var col4Offset = 350;
                var col5Offset = 460;
                var col6Offset = 589;

                gfx.DrawLine(new XPen(XBrushes.Black, 1), new XPoint(col1Offset, tableYStart + rowHeight), new XPoint(col1Offset, tableYStart + rowHeight * (order.OrderDetails.Count + 3)));
                gfx.DrawLine(new XPen(XBrushes.Black, 1), new XPoint(col2Offset, tableYStart + rowHeight), new XPoint(col2Offset, tableYStart + rowHeight * (order.OrderDetails.Count + 3)));
                gfx.DrawLine(new XPen(XBrushes.Black, 1), new XPoint(col3Offset, tableYStart + rowHeight), new XPoint(col3Offset, tableYStart + rowHeight * (order.OrderDetails.Count + 2)));
                gfx.DrawLine(new XPen(XBrushes.Black, 1), new XPoint(col4Offset, tableYStart + rowHeight), new XPoint(col4Offset, tableYStart + rowHeight * (order.OrderDetails.Count + 2)));
                gfx.DrawLine(new XPen(XBrushes.Black, 1), new XPoint(col5Offset, tableYStart + rowHeight), new XPoint(col5Offset, tableYStart + rowHeight * (order.OrderDetails.Count + 3)));
                gfx.DrawLine(new XPen(XBrushes.Black, 1), new XPoint(col6Offset, tableYStart + rowHeight), new XPoint(col6Offset, tableYStart + rowHeight * (order.OrderDetails.Count + 3)));

                gfx.DrawString(
                "STT", font, XBrushes.Black,
                new XRect(col1Offset, tableYStart + rowHeight, col2Offset - col1Offset, rowHeight),
                XStringFormats.Center);

                gfx.DrawString(
                    "Tên sản phẩm", font, XBrushes.Black,
                    new XRect(col2Offset, tableYStart + rowHeight, col3Offset - col2Offset, rowHeight),
                    XStringFormats.Center);

                gfx.DrawString(
                    "Số lượng", font, XBrushes.Black,
                    new XRect(col3Offset, tableYStart + rowHeight, col4Offset - col3Offset, rowHeight),
                    XStringFormats.Center);

                gfx.DrawString(
                    "Đơn giá", font, XBrushes.Black,
                    new XRect(col4Offset, tableYStart + rowHeight, col5Offset - col4Offset, rowHeight),
                    XStringFormats.Center);


                gfx.DrawString(
                    "Thành tiền", font, XBrushes.Black,
                    new XRect(col5Offset, tableYStart + rowHeight, col6Offset - col5Offset, rowHeight),
                    XStringFormats.Center);

                if (order.OrderDetails == null) return "error";

                var orderDetails = order.OrderDetails;
                decimal total = 0;
                for (int i = 1; i <= orderDetails.Count; i++)
                {
                    var money = orderDetails[i - 1].SellPrice * orderDetails[i - 1].Quantity;
                    total += money;
                    gfx.DrawString(
                        $"{i}", font, XBrushes.Black,
                        new XRect(col1Offset, tableYStart + rowHeight * (1 + i), col2Offset - col1Offset, rowHeight),
                        XStringFormats.Center);

                    gfx.DrawString(
                        $"{orderDetails[i - 1].Product?.Name} ({orderDetails[i - 1].Color?.Name}, {orderDetails[i - 1].Capacity?.Ram}/{orderDetails[i - 1].Capacity?.Rom})", fontsm, XBrushes.Black,
                        new XRect(col2Offset, tableYStart + rowHeight * (1 + i), col3Offset - col2Offset, rowHeight),
                        XStringFormats.Center);

                    gfx.DrawString(
                        $"{orderDetails[i - 1].Quantity}", font, XBrushes.Black,
                        new XRect(col3Offset + 10, tableYStart + rowHeight * (1 + i), col4Offset - col3Offset, rowHeight),
                        XStringFormats.Center);

                    gfx.DrawString(
                        $"{orderDetails[i - 1].SellPrice.ToString("N0", new CultureInfo("vi-VN"))}đ", font, XBrushes.Black,
                        new XRect(col4Offset + 10, tableYStart + rowHeight * (1 + i), col5Offset - col4Offset, rowHeight),
                        XStringFormats.Center);

                    gfx.DrawString(
                        $"{money.ToString("N0", new CultureInfo("vi-VN"))}đ", font, XBrushes.Black,
                        new XRect(col5Offset + 10, tableYStart + rowHeight * (1 + i), col6Offset - col5Offset, rowHeight),
                        XStringFormats.Center);


                    if (i == orderDetails.Count)
                    {
                        gfx.DrawString(
                        "Tổng cộng", font, XBrushes.Black,
                        new XRect(col2Offset + 10, tableYStart + rowHeight * (2 + i), col3Offset - col2Offset, rowHeight),
                        XStringFormats.Center);

                        gfx.DrawString(
                        $"{total.ToString("N0", new CultureInfo("vi-VN"))}đ", font, XBrushes.Black,
                        new XRect(col5Offset + 10, tableYStart + rowHeight * (2 + i), col6Offset - col5Offset, rowHeight),
                        XStringFormats.Center);
                    }
                }

                for (int i = 0; i <= orderDetails.Count + 2; i++)
                {
                    gfx.DrawLine(new XPen(XBrushes.Black, 1), new XPoint(20, tableYStart + rowHeight * (i + 1)), new XPoint(page.Width - 20, tableYStart + rowHeight * (i + 1)));
                }

                gfx.DrawString(
                    $"Tổng tiền: {total.ToString("N0", new CultureInfo("vi-VN"))}đ", font, XBrushes.Black,
                    new XRect(20, tableYStart + rowHeight * (orderDetails.Count() + 3), page.Width / 2, rowHeight),
                    XStringFormats.CenterLeft);

                gfx.DrawString(
                    "KHÁCH HÀNG", font, XBrushes.Black,
                    new XRect(0, page.Height - 140, page.Width / 2, 14),
                    XStringFormats.Center);

                gfx.DrawString(
                    "Ngày 11 tháng 11 năm 2023", fontItalic, XBrushes.Black,
                    new XRect(300, page.Height - 160, page.Width / 2, 14),
                    XStringFormats.Center);

                gfx.DrawString(
                    "NGƯỜI BÁN HÀNG", font, XBrushes.Black,
                    new XRect(300, page.Height - 140, page.Width / 2, 14),
                    XStringFormats.Center);

                string filepath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "Products", "OrderPdf", order.Code + ".pdf");
                document.Save(filepath);

                string path = "\\" + Path.Combine("files", "Products", "OrderPdf", order.Code + ".pdf");

                return path;
            }
            catch (Exception ex)
            {
                return $"error: {ex.Message}";
            }
        }
    }
}