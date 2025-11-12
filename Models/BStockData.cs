namespace IPOPulse.Models
{
    public class BStockData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Symbol { get; set; }
        public string BuyingPrice { get; set; }
        public string? ExitPrice { get; set; }
        public DateTime Date { get; set; }
        public string CurrentPrice {  get; set; }
        public string Returns {  get; set; }
        public string SL {  get; set; }
    }
}
