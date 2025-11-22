using IPOPulse.Authorize;
using IPOPulse.DBContext;
using IPOPulse.Models;
using Microsoft.AspNetCore.Mvc;

namespace IPOPulse.Controllers
{
    [SessionAuthorize]
    public class DashboardController : Controller
    {
        private readonly AppDBContext _context;
        public DashboardController(AppDBContext context)
        {
              _context = context;
        }
        public IActionResult Index()
        {
            List<BStockData> data = _context.BStocks
                                            .Where(s => s.ExitPrice==null)
                                            .ToList();
            return View(data);
        }

        public IActionResult History()
        {
            List<BStockData> data = _context.BStocks
                                            .Where(s => s.ExitPrice != null)
                                            .ToList();
            return View(data);
        }

        [HttpPost]
        public async Task<IActionResult> SqOff(String Id)
        {
            BStockData stock = _context.BStocks.FirstOrDefault(st => st.Id==Id);
            if (stock == null)
            {
                return RedirectToAction("Error", "Home");
            }
            stock.ExitPrice = stock.CurrentPrice;
            await _context.SaveChangesAsync();
            return RedirectToAction("History");
        }
    }
}
