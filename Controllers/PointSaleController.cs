using Microsoft.AspNetCore.Mvc;
using System;
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

        public PointSaleController()
        {
            _merchants = new List<PointSale>(); // Инициализация списка точек продажи
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchPointSale(string searchTerm)
        {
            // Получение списка точек продажи из JSON данных
            _merchants = await PointSale.GetPointSalesFromJson("https://localhost:7089/api/MyApi");

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
            _merchants = await PointSale.GetPointSalesFromJson("https://localhost:7089/api/MyApi");

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

        [HttpPost]
        public IActionResult GenerateQRCode(List<string> qrCodeUrls)
        {
            // Проверяем, что были переданы ссылки на QR-коды
            if (qrCodeUrls != null && qrCodeUrls.Count > 0)
            {
                // Создаем новый документ Word
                XWPFDocument doc = new XWPFDocument();

                // Добавляем каждую ссылку на QR-код в документ Word
                foreach (string url in qrCodeUrls)
                {
                    // Получаем изображение QR-кода по ссылке
                    byte[] qrCodeBytes = GetQRCodeImageBytes(url);

                    if (qrCodeBytes != null)
                    {
                        // Создание параграфа и добавление изображения QR-кода в документ Word
                        var paragraph = doc.CreateParagraph();
                        var run = paragraph.CreateRun();

                        using (MemoryStream ms = new MemoryStream(qrCodeBytes))
                        {
                            run.AddPicture(ms, (int)PictureType.PNG, "QR_Code.png", Units.ToEMU(500), Units.ToEMU(500));
                        }
                        run.AddBreak(BreakType.PAGE); // Создание разрыва страницы
                    }
                }

                // Сохранение документа Word в файле QR_Code.docx
                string filePath = "QR_Code.docx";
                using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    doc.Write(fs);
                }

                // Возвращаем результат скачивания файла
                byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
                string fileName = "QR_Code.docx";
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", fileName);
            }

            // Перенаправление на страницу точек продажи после сохранения QR-кодов
            return RedirectToAction("CreatePointSale");
        }

    }
}
