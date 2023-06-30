using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace AdminQRCodeMVC.Models
{
    public class PointSale
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int State { get; set; }
        public string qr_data { get; set; }

        public PointSale()
        {
            // Конструктор по умолчанию
        }

        public static async Task<List<PointSale>> GetPointSalesFromJson(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    var data = JsonConvert.DeserializeObject<JSonPat>(json);
                    if (data?.merchants != null)
                    {
                        return data.merchants;
                    }
                }
            }

            return new List<PointSale>();
        }
    }
}
