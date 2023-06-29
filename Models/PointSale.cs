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
    }
}
