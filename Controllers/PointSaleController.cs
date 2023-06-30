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

        [HttpPost]
        public IActionResult GenerateQRCode(int id)
        {
            if (_merchants == null)
            {
                // Если _merchants не инициализирован, выполните соответствующие действия или верните ошибку
                return RedirectToAction("CreatePointSale");
            }

            // Создаем список ссылок на QR-коды
            List<string> qrCodeLinks = new List<string>();

            foreach (var pointSale in _merchants)
            {
                // Создание qr-кода на основе данных точки продажи
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(pointSale.qr_data, QRCodeGenerator.ECCLevel.Q);
                QRCode qrCode = new QRCode(qrCodeData);

                using (Bitmap qrCodeImage = qrCode.GetGraphic(10))
                using (MemoryStream ms = new MemoryStream())
                {
                    qrCodeImage.Save(ms, ImageFormat.Png);
                    byte[] qrCodeBytes = ms.ToArray();

                    // Преобразование изображения qr-кода в строку Base64
                    string qrCodeImageBase64 = Convert.ToBase64String(qrCodeBytes);

                    // Добавляем ссылку на QR-код в список
                    qrCodeLinks.Add(qrCodeImageBase64);
                }
            }

            // Сохранение ссылок на QR-коды в файле "QR_Code.docx"
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "QR_Code.docx");

            XWPFDocument doc = new XWPFDocument();
            foreach (var qrCodeLink in qrCodeLinks)
            {
                XWPFParagraph paragraph = doc.CreateParagraph();
                XWPFRun run = paragraph.CreateRun();
                run.SetText(qrCodeLink);
                doc.CreateParagraph(); // Добавляем пустой абзац между ссылками
            }

            using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                doc.Write(fs);
            }

            // Перенаправление на страницу точек продажи после сохранения qr-кодов
            return RedirectToAction("CreatePointSale");
        }
    }
}
