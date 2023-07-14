using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AdminQRCodeMVC.Models;
using QRCoder;
using System.Drawing.Imaging;
using System.Drawing;
using NPOI.XWPF.UserModel;
using NPOI.Util;

namespace AdminQRCodeMVC.Controllers
{
    public class PointSaleController : Controller
    {
        private List<PointSale> _merchants; // Определение списка точек продажи
        private readonly IConfiguration _configuration;
        private readonly string _apiUrl;

        public PointSaleController(IConfiguration configuration)
        {
            _configuration = configuration;
            _apiUrl = _configuration["ApiUrls:MyApi"];
        
            
            // Инициализация списка точек продажи
            _merchants = new List<PointSale>();

            // Заполнение списка точек продажи из JSON данных
            FillMerchantsList();
        }

        private async Task FillMerchantsList()
        {
            _merchants = await PointSale.GetPointSalesFromJson(_apiUrl);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchPointSale(string searchTerm)
        {
            // Получение списка точек продажи из JSON данных
            _merchants = await PointSale.GetPointSalesFromJson(_apiUrl);

            if (string.IsNullOrEmpty(searchTerm))
            {
                // Вернуть все записи
                return View("CreatePointSale", _merchants);
            }

            // Преобразование поискового запроса в нижний регистр для сравнения без учета регистра
            string searchTermLower = searchTerm.ToLower();

            // Выполнение поиска по Id и Title
            var searchResults = _merchants.Where(merchant =>
                merchant.Id.ToString().Contains(searchTermLower) ||
                merchant.Title.ToLower().Contains(searchTermLower)
            ).ToList();

            return View("CreatePointSale", searchResults);
        }

        public async Task<IActionResult> CreatePointSale()
        {
            // Получение списка точек продажи из JSON данных
            _merchants = await PointSale.GetPointSalesFromJson(_apiUrl);

            if (_merchants.Count > 0)
            {
                return View("CreatePointSale", _merchants);
            }

            return View("CreatePointSale", new List<PointSale>());
        }

        private byte[] GetQRCodeImageBytes(string qrCodeUrl)
        {
            // Реализуйте логику для получения байтов изображения QR-кода по URL
            // Возвращайте null, если не удалось получить изображение

            // Пример реализации, используя QRCodeGenerator:
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrCodeUrl, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);

            using (Bitmap qrCodeImage = qrCode.GetGraphic(20))
            using (MemoryStream ms = new MemoryStream())
            {
                qrCodeImage.Save(ms, ImageFormat.Png);
                return ms.ToArray();
            }
        }

        [HttpGet("qr-code/{url}")]
        public IActionResult GetQRCodeImage(string url)
        {
            byte[] qrCodeBytes = GetQRCodeImageBytes(url);

            if (qrCodeBytes != null)
            {
                return File(qrCodeBytes, "image/png");
            }

            // Вернуть изображение-заглушку или другое сообщение об ошибке
            return NotFound();
        }

        private byte[] SaveWordDocument(XWPFDocument doc, string fileName)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                doc.Write(ms);
                return ms.ToArray();
            }
        }

        [HttpPost]
        public IActionResult GenerateQRCode(List<string> qrCodeUrls) // Сохранение всех отфильтрованных записей
        {
            // Проверяем, что были переданы ссылки на QR-коды
            if (qrCodeUrls != null && qrCodeUrls.Count > 0)
            {
                // Создаем новый документ Word
                XWPFDocument doc = new XWPFDocument();

                // Добавляем каждую запись с QR-кодом и данными в документ Word
                for (int i = 0; i < qrCodeUrls.Count; i++)
                {
                    string qrCodeUrl = qrCodeUrls[i];

                    // Получаем изображение QR-кода по ссылке
                    byte[] qrCodeBytes = GetQRCodeImageBytes(qrCodeUrl);

                    if (qrCodeBytes != null)
                    {
                        // Создание параграфа и добавление данных записи в документ Word
                        var paragraph = doc.CreateParagraph();
                        paragraph.Alignment = ParagraphAlignment.LEFT;

                        var run = paragraph.CreateRun();
                        run.FontSize = 14;
                        run.IsBold = true;
                        run.SetText("ID: " + _merchants[i].Id);

                        paragraph = doc.CreateParagraph();
                        paragraph.Alignment = ParagraphAlignment.LEFT;

                        run = paragraph.CreateRun();
                        run.FontSize = 14;
                        run.IsBold = true;
                        run.SetText("Название: " + _merchants[i].Title);

                        paragraph = doc.CreateParagraph();
                        paragraph.Alignment = ParagraphAlignment.LEFT;

                        run = paragraph.CreateRun();
                        run.FontSize = 14;
                        run.IsBold = true;
                        run.SetText("Статус: " + _merchants[i].State);

                        // Добавление изображения QR-кода в документ Word
                        paragraph = doc.CreateParagraph();
                        paragraph.Alignment = ParagraphAlignment.CENTER;

                        run = paragraph.CreateRun();

                        using (MemoryStream ms = new MemoryStream(qrCodeBytes))
                        {
                            run.AddPicture(ms, (int)PictureType.PNG, "QR_Code.png", Units.ToEMU(300), Units.ToEMU(300));
                        }

                        // Добавление ссылки в документ Word
                        paragraph = doc.CreateParagraph();
                        paragraph.Alignment = ParagraphAlignment.LEFT;

                        run = paragraph.CreateRun();
                        run.FontSize = 14;
                        run.IsBold = true;
                        run.SetText("Ссылка: " + qrCodeUrl);

                        run.AddBreak(BreakType.PAGE); // Создание разрыва страницы
                    }
                }

                // Сохранение документа Word в байтовый массив
                string fileName = "QR_Code.docx";
                byte[] documentBytes = SaveWordDocument(doc, fileName);

                // Возвращаем результат скачивания файла
                return File(documentBytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", fileName);
            }

            // Перенаправление на страницу точек продажи после сохранения QR-кодов
            return RedirectToAction("CreatePointSale");
        }

        [HttpPost]
        public IActionResult SaveOneQRCode(int index) // Сохранение одной записи
        {
            if (index >= 0 && index < _merchants.Count)
            {
                PointSale pointSale = _merchants[index];

                // Создание документа Word для одной записи и QR-кода
                XWPFDocument doc = new XWPFDocument();

                // Добавление данных записи в документ Word
                var paragraph = doc.CreateParagraph();
                paragraph.Alignment = ParagraphAlignment.LEFT;

                var run = paragraph.CreateRun();
                run.FontSize = 14;
                run.IsBold = true;
                run.SetText("ID: " + pointSale.Id);

                paragraph = doc.CreateParagraph();
                paragraph.Alignment = ParagraphAlignment.LEFT;

                run = paragraph.CreateRun();
                run.FontSize = 14;
                run.IsBold = true;
                run.SetText("Название: " + pointSale.Title);

                paragraph = doc.CreateParagraph();
                paragraph.Alignment = ParagraphAlignment.LEFT;

                run = paragraph.CreateRun();
                run.FontSize = 14;
                run.IsBold = true;
                run.SetText("Статус: " + pointSale.State);

                // Добавление изображения QR-кода в документ Word
                byte[] qrCodeBytes = GetQRCodeImageBytes(pointSale.QrData);

                if (qrCodeBytes != null)
                {
                    paragraph = doc.CreateParagraph();
                    paragraph.Alignment = ParagraphAlignment.CENTER;

                    run = paragraph.CreateRun();

                    using (MemoryStream ms = new MemoryStream(qrCodeBytes))
                    {
                        run.AddPicture(ms, (int)PictureType.PNG, "QR_Code.png", Units.ToEMU(300), Units.ToEMU(300));
                    }
                }

                // Добавление ссылки в документ Word
                paragraph = doc.CreateParagraph();
                paragraph.Alignment = ParagraphAlignment.LEFT;

                run = paragraph.CreateRun();
                run.FontSize = 14;
                run.IsBold = true;
                run.SetText("Ссылка: " + pointSale.QrData);

                // Сохранение документа Word в байтовый массив
                string fileName = $"QR_Code_{index}.docx";
                byte[] documentBytes = SaveWordDocument(doc, fileName);

                // Возвращаем результат скачивания файла
                return File(documentBytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", fileName);
            }

            // Если не удалось сохранить QR-код, перенаправляем на страницу точек продажи
            return RedirectToAction("CreatePointSale");
        }

    }
}
