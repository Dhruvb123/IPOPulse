using Microsoft.EntityFrameworkCore;

namespace IPOPulse.Models
{
    public class MarketData
    {
        public string? ISIN { get; set; }
        public string? Name { get; set; }
        public string? OfferedPrice { get; set; }
        public string? listingDayHigh {  get; set; }
        public string? listingDayLow { get; set; }
        public string? currentPrice {  get; set; }
        public int counter { get; set; }

    }
}
