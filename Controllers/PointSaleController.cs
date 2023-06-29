using Microsoft.AspNetCore.Mvc;
using AdminQRCodeMVC.Models;
using Newtonsoft.Json;
using QRCoder;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using NPOI.XWPF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using NPOI.Util;

namespace AdminQRCodeMVC.Controllers
{
    //I was here
    [ApiController]
    [Route("api/pointsale")]
    public class PointSaleController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> CreatePointSale()
        {
            string url = "https://localhost:7089/api/MyApi";
            string documentPath = "QR_Code.docx";

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    var data = JsonConvert.DeserializeObject<JSonPat>(json);

                    if (data.merchants != null && data.merchants.Count > 0)
                    {
                        XWPFDocument document;
                        if (System.IO.File.Exists(documentPath))
                        {
                            using (FileStream fs = new FileStream(documentPath, FileMode.Open, FileAccess.ReadWrite))
                            {
                                document = new XWPFDocument(fs);
                                fs.SetLength(0); // Очистить содержимое файла
                            }
                        }
                        else
                        {
                            document = new XWPFDocument();
                        }

                        foreach (var merchant in data.merchants)
                        {
                            if (!string.IsNullOrEmpty(merchant.qr_data))
                            {
                                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                                QRCodeData qrCodeData = qrGenerator.CreateQrCode(merchant.qr_data, QRCodeGenerator.ECCLevel.Q);
                                QRCode qrCode = new QRCode(qrCodeData);
                                Bitmap qrCodeImage = qrCode.GetGraphic(20);

                                using (MemoryStream imageStream = new MemoryStream())
                                {
                                    qrCodeImage.Save(imageStream, ImageFormat.Png);
                                    imageStream.Position = 0;

                                    XWPFParagraph paragraph = document.CreateParagraph();
                                    XWPFRun run = paragraph.CreateRun();
                                    run.AddPicture(imageStream, (int)NPOI.XWPF.UserModel.PictureType.PNG, "QR_Code", Units.ToEMU(200), Units.ToEMU(200));
                                }
                            }
                        }

                        Directory.CreateDirectory(Path.GetDirectoryName(documentPath));
                        using (FileStream fs = new FileStream(documentPath, FileMode.Create))
                        {
                            document.Write(fs);
                        }

                        byte[] fileBytes = System.IO.File.ReadAllBytes(documentPath);

                        ViewBag.MyData = "Some data"; // Передача значения в представление

                        return View("CreatePointSale", fileBytes); // Возвращает представление с переданным значением
                    }
                    else
                    {
                        return NotFound("Нет доступных данных");
                    }
                }
            }

            return NotFound("Нет доступных данных");
        }
    }
}
