namespace FormRecognizerLab.Functions.Models
{
    public class InvoceModel
    {
        public string CompanyPhone { get; set; }
        public string WebSite { get; set; }
        public string Email { get; set; }
        public string Date { get; set; }
        public string ShippedToVendor { get; set; }
        public string ShippedtoCompany { get; set; }
        public string ShippedFromCompany { get; set;}
        public string Subtotal { get; set; }
        public string Tax { get; set; }
        public string Total { get; set; }

        public string PurchaseOrder { get; set; }
    }
}
