using Microsoft.EntityFrameworkCore;

namespace IPOPulse.Models
{
    public class IPOData
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Symbol { get; set; }
        public DateTime ListingDate {  get; set; }
        public string? Price {  get; set; }
    }
}
