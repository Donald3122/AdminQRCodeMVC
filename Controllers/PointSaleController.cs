using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AdminQRCodeMVC.Models;

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
    }
}
