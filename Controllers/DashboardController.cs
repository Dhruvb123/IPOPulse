using IPOPulse.DBContext;
using IPOPulse.Models;
using Microsoft.AspNetCore.Mvc;

namespace IPOPulse.Controllers
{
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
    }
}
