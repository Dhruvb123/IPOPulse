using Microsoft.EntityFrameworkCore;

namespace IPOPulse.Models
{
    public class MarketData
    {
        public string ISIN { get; set; }
        public string Name { get; set; }
        public string Symbol { get; set; }
        public string offeredPrice { get; set; }
        public string listingDayHigh {  get; set; }
        public string listingDayLow { get; set; }
        public string currentPrice {  get; set; }
        public int counter { get; set; }
        public string ID {  get; set; }
        public DateTime ListingDate { get; set; }

    }
}
