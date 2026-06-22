namespace MMarketApp.Models
{
    public class Product
    {
        public int ProductID { get; set; }
        public string Article { get; set; }
        public string Name { get; set; }
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public string Brand { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string Unit { get; set; }
        public int? ExpiryDate { get; set; }
    }
}