using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IbukunOluwaPharmaMart.Data;
using IbukunOluwaPharmaMart.Models;

namespace IbukunOluwaPharmaMart.Controllers
{
    public class SalesController : Controller
    {
        private readonly AppDbContext _context;
        public SalesController(AppDbContext context) => _context = context;

        // --- ADDED: Helper to check role ---
        private bool IsCashier() => HttpContext.Session.GetString("Role") == "Cashier";
        private bool IsViewer() => HttpContext.Session.GetString("Role") == "Viewer";
        private bool IsNotLoggedIn() => string.IsNullOrEmpty(HttpContext.Session.GetString("Username"));
        

        // --- CHANGED: Report = TODAY ONLY ---
        public async Task<IActionResult> Report()
        {
            // --- ADDED: BLOCK NOT LOGGED IN ---
            if (IsNotLoggedIn()) return RedirectToAction("Login", "Auth");

            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            var query = _context.Sales
                .Include(s => s.Items)
                .ThenInclude(i => i.Product)
                .Where(s => s.Date >= today && s.Date < tomorrow)
                .AsQueryable();

            var sales = await query.OrderByDescending(s => s.Date).ToListAsync();

            ViewBag.TotalSales = sales.Sum(s => s.TotalAmount);
            ViewBag.TotalTransactions = sales.Count;
            ViewBag.TotalItems = sales.Sum(s => s.Items == null ? 0 : s.Items.Sum(i => i.Qty));
            ViewBag.TopProduct = sales
                .SelectMany(s => s.Items ?? new List<SaleItem>())
                .GroupBy(i => i.Product != null ? i.Product.Name : "Unknown")
                .OrderByDescending(g => g.Sum(i => i.Qty))
                .Select(g => g.Key)
                .FirstOrDefault() ?? "N/A";

            // --- ADDED: FIX - Hide Profit from Cashier, show only for Admin/Manager ---
            if (IsCashier()|| IsViewer())
            {
                ViewBag.TotalProfit = 0;
                ViewBag.IsCashier = true;
            }
            else
            {
                ViewBag.TotalProfit = sales.Sum(s =>
                    s.Items == null ? 0 : s.Items.Sum(i =>
                        (i.Price - (i.Product != null ? i.Product.CostPrice : 0)) * i.Qty
                    )
                );
                ViewBag.IsCashier = false;
            }

            // Kept for safety, will be only today
            ViewBag.DailyTotals = sales
              .GroupBy(s => s.Date.Date)
              .Select(g => new {
                  Date = g.Key,
                  Count = g.Count(),
                  Total = g.Sum(x => x.TotalAmount)
              })
              .OrderByDescending(x => x.Date)
              .ToList();

            return View(sales);
        }

        // --- NEW: History = From Date to To Date ---
        public async Task<IActionResult> History(DateTime? from, DateTime? to)
        {
            // --- ADDED: BLOCK NOT LOGGED IN ---
            if (IsNotLoggedIn()) return RedirectToAction("Login", "Auth");

            var query = _context.Sales
                .Include(s => s.Items)
                .ThenInclude(i => i.Product)
                .AsQueryable();

            if (from.HasValue) query = query.Where(s => s.Date >= from.Value.Date);
            if (to.HasValue) query = query.Where(s => s.Date <= to.Value.Date.AddDays(1).AddTicks(-1));

            var sales = await query.OrderByDescending(s => s.Date).ToListAsync();

            ViewBag.TotalSales = sales.Sum(s => s.TotalAmount);
            ViewBag.TotalTransactions = sales.Count;
            ViewBag.TotalItems = sales.Sum(s => s.Items == null ? 0 : s.Items.Sum(i => i.Qty));
            ViewBag.TopProduct = sales
                .SelectMany(s => s.Items ?? new List<SaleItem>())
                .GroupBy(i => i.Product != null ? i.Product.Name : "Unknown")
                .OrderByDescending(g => g.Sum(i => i.Qty))
                .Select(g => g.Key)
                .FirstOrDefault() ?? "N/A";
            ViewBag.From = from?.ToString("yyyy-MM-dd");
            ViewBag.To = to?.ToString("yyyy-MM-dd");

            // --- ADDED: FIX - Hide Profit from Cashier, show only for Admin/Manager ---
            if (IsCashier()|| IsViewer())
            {
                ViewBag.TotalProfit = 0;
                ViewBag.IsCashier = true;
            }
            else
            {
                ViewBag.TotalProfit = sales.Sum(s =>
                    s.Items == null ? 0 : s.Items.Sum(i =>
                        (i.Price - (i.Product != null ? i.Product.CostPrice : 0)) * i.Qty
                    )
                );
                ViewBag.IsCashier = false;
            }

            ViewBag.DailyTotals = sales
              .GroupBy(s => s.Date.Date)
              .Select(g => new {
                  Date = g.Key,
                  Count = g.Count(),
                  Total = g.Sum(x => x.TotalAmount)
              })
              .OrderByDescending(x => x.Date)
              .ToList();

            return View(sales);
        }

        public async Task<IActionResult> Receipt(int id)
        {
            // --- ADDED: BLOCK NOT LOGGED IN ---
            if (IsNotLoggedIn()) return RedirectToAction("Login", "Auth");

            var sale = await _context.Sales
                .Include(s => s.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (sale == null) return NotFound();
            return View(sale);
        }

        // --- NEW: 10. Daily Closing / Shift Report - WITH Opening + Expenses ---
        public async Task<IActionResult> CloseDay(DateTime? date, decimal openingCash = 0, decimal expenses = 0)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("Username")))
                return RedirectToAction("Login", "Auth");

            var day = (date ?? DateTime.Today).Date;
            var nextDay = day.AddDays(1);

            var sales = await _context.Sales
                .Include(s => s.Items)
                .ThenInclude(i => i.Product)
                .Where(s => s.Date >= day && s.Date < nextDay)
                .OrderByDescending(s => s.Date)
                .ToListAsync();

            var totalSales = sales.Sum(s => s.TotalAmount);
            var totalCollected = sales.Sum(s => s.AmountPaid);
            var totalOwing = sales.Sum(s => s.Balance);

            var cashCollected = sales.Where(s => s.PaymentType == "Cash").Sum(s => s.AmountPaid);
            var transferCollected = sales.Where(s => s.PaymentType == "Transfer").Sum(s => s.AmountPaid);
            var posCollected = sales.Where(s => s.PaymentType == "POS" || s.PaymentType == "Card").Sum(s => s.AmountPaid);

            // --- NEW: Real Cash to Remit ---
            // Example: Cash Collected 50,000 + Opening 10,000 - Expenses 2,000 = 58,000 to remit
            var cashToRemit = cashCollected + openingCash - expenses;

            ViewBag.Day = day;
            ViewBag.TotalSales = totalSales;
            ViewBag.TotalCollected = totalCollected;
            ViewBag.TotalOwing = totalOwing;
            ViewBag.CashCollected = cashCollected;
            ViewBag.TransferCollected = transferCollected;
            ViewBag.PosCollected = posCollected;
            ViewBag.TransactionCount = sales.Count;
            ViewBag.CashierName = HttpContext.Session.GetString("Username");
            ViewBag.Role = HttpContext.Session.GetString("Role");

            // --- NEW ---
            ViewBag.OpeningCash = openingCash;
            ViewBag.Expenses = expenses;
            ViewBag.CashToRemit = cashToRemit;

            return View(sales);
        }
    }
}
