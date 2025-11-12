using IPOPulse.DBContext;
using IPOPulse.Models;

namespace IPOPulse.Services
{
    public class AlertService
    {
        private readonly AppDBContext _context;

        public AlertService(AppDBContext context)
        {
            _context = context;
        }

        public void UpdateCurrentPrice(BStockData bstock, string price)
        {
            bstock.CurrentPrice = price;
            var buy = decimal.Parse(bstock.BuyingPrice);
            var cur = decimal.Parse(bstock.CurrentPrice);
            var change = (cur-buy) / buy;
            change *= 100;

            bstock.Returns = change + "%";
        }

        public async void CheckSLAndTarget()
        {
            List<BStockData> list = _context.BStocks.ToList();
            foreach (var item in list)
            {
                if (item.ExitPrice != null)
                {
                    continue;
                }
                var curr = decimal.Parse(item.CurrentPrice);
                if ( curr <= decimal.Parse(item.SL))
                {
                    // Send Sell Alert
                    await SellAlert(item, 0);
                }
                var bPrice = decimal.Parse(item.BuyingPrice);
                if (curr >= bPrice + (0.2m * bPrice)) { 
                    await SellAlert(item, 1);
                }

            }
        }

        public async Task BuyAlert(MarketData stock, IPOData st)
        {
            BStockData bstock= new BStockData()
            {
                Id = stock.ID,
                Name = stock.Name,
                Symbol = st.Symbol,
                BuyingPrice = stock.currentPrice,
                Date = DateTime.Now,
                CurrentPrice = stock.currentPrice,
                Returns = "0%",
                SL = stock.listingDayLow
            };

            _context.BStocks.Add(bstock);
            await _context.SaveChangesAsync();
        }

        public async Task SellAlert(BStockData stock, int indicator)
        {
            stock.ExitPrice = stock.CurrentPrice;
            if (indicator == 0)
            {
                // Fn to sell alert msg doe to SL hit
            }
            else
            {
                // Sell Recommendation Msg for Profit Booking
            }
        }
    }
}
