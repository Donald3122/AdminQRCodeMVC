namespace AdminQRCodeMVC.Models
{
    public class JSonPat
    {
        public int errCode { get; set; }
        public List<PointSale> merchants { get; set; }

        public JSonPat()
        {
            // Конструктор по умолчанию
        }
    }
}
