using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IbukunOluwaPharmaMart.Data;

namespace IbukunOluwaPharmaMart.Controllers
{
    public class SalesController : Controller
    {
        private readonly AppDbContext _context;
        public SalesController(AppDbContext context) => _context = context;

        public async Task<IActionResult> Report(DateTime? from, DateTime? to)
        {
            var query = _context.Sales.Include(s => s.Items).ThenInclude(i => i.Product).AsQueryable();
            if (from.HasValue) query = query.Where(s => s.Date >= from.Value);
            if (to.HasValue) query = query.Where(s => s.Date <= to.Value);

            var sales = await query.OrderByDescending(s => s.Date).ToListAsync();

            ViewBag.TotalSales = sales.Sum(s => s.TotalAmount);
            ViewBag.TotalProfit = sales.Sum(s => s.Items.Sum(i => (i.SellingPrice - (i.Product.CostPrice)) * i.Quantity));
            ViewBag.TotalTransactions = sales.Count;

            return View(sales);
        }
    }
}