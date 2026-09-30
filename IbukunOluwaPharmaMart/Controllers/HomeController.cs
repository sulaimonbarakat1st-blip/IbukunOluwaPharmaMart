using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IbukunOluwaPharmaMart.Data;

namespace IbukunOluwaPharmaMart.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(AppDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalProducts = await _context.Products.CountAsync();
            ViewBag.LowStock = await _context.Products.CountAsync(p => p.Quantity <= p.LowStockLevel);
            ViewBag.Categories = await _context.Categories.ToListAsync();
            var featured = await _context.Products.Include(p => p.Category).Take(8).ToListAsync();
            return View(featured);
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}