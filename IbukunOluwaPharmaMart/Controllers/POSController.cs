using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IbukunOluwaPharmaMart.Data;
using IbukunOluwaPharmaMart.Models;

namespace IbukunOluwaPharmaMart.Controllers
{
    public class POSController : Controller
    {
        private readonly AppDbContext _context;
        public POSController(AppDbContext context) { _context = context; }
        private bool IsViewer() => HttpContext.Session.GetString("Role") == "Viewer";

        public async Task<IActionResult> Sell()
        {
            // FIX 1: BLOCK IF NOT LOGGED IN - ADDED
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("Username")))
                return RedirectToAction("Login", "Auth");

            var products = await _context.Products
                .Where(p => p.ExpiryDate == null || p.ExpiryDate > DateTime.Today) // hide expired from sell
                .OrderBy(p => p.Name).ToListAsync();

            // FIX 2: PASS ROLE TO VIEW TO HIDE COST PRICE - ADDED
            ViewBag.Role = HttpContext.Session.GetString("Role");
            ViewBag.FullName = HttpContext.Session.GetString("FullName");

            return View(products);
        }
        public async Task<IActionResult> Search(string term)
        {
            // FIX 3: SEARCH ALSO NEEDS LOGIN - ADDED
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("Username")))
                return Json(new List<object>());

            var products = await _context.Products
              .Where(p => p.Name.Contains(term) && p.Quantity > 0)
              .Select(p => new {
                  p.Id,
                  p.Name,
                  RetailPrice = p.RetailPrice,
                  WholesalePrice = p.WholesalePrice,
                  p.Quantity,
                  p.Dosage,
                  // FIX 4: DON'T SEND COST PRICE TO CASHIER - ADDED
                  CostPrice = (HttpContext.Session.GetString("Role") == "Cashier" || HttpContext.Session.GetString("Role") == "Viewer") ? 0 : p.CostPrice
              })
              .Take(10).ToListAsync();
            return Json(products);
        }

        [HttpPost]
        public async Task<IActionResult> Checkout([FromBody] CheckoutRequestDto request)
        {
            if (IsViewer()) return Json(new { success = false, message = "Viewer cannot do this - view only!" });
            if (request.Items == null || !request.Items.Any()) return Json(new { success = false, message = "Cart empty" });

            var sale = new Sale
            {
                ReceiptNo = "REC-" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                Date = DateTime.Now,
                TotalAmount = 0,
                PaymentType = request.PaymentType,
                AmountPaid = request.AmountPaid,
                CustomerName = request.CustomerName,
                CustomerPhone = request.CustomerPhone
            };
            _context.Sales.Add(sale);
            await _context.SaveChangesAsync();

            decimal total = 0;
            foreach (var item in request.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product == null || product.Quantity < item.Quantity) continue;
                product.Quantity -= item.Quantity;

                // FIX: Use price from POS (retail/wholesale), fallback to RetailPrice, then SellingPrice
                decimal unitPrice = item.Price > 0 ? item.Price : (product.RetailPrice > 0 ? product.RetailPrice : product.SellingPrice);
                if (unitPrice == 0) unitPrice = product.SellingPrice;

                total += unitPrice * item.Quantity;
                _context.SaleItems.Add(new SaleItem { SaleId = sale.Id, ProductId = product.Id, Qty = item.Quantity, Price = unitPrice });
            }
            sale.Balance = total - request.AmountPaid;
            if (sale.Balance < 0) sale.Balance = 0;
            sale.IsFullyPaid = sale.Balance <= 0;
            sale.TotalAmount = total;
            await _context.SaveChangesAsync();
            return Json(new { success = true, receiptNo = sale.ReceiptNo });
        }

        [HttpPost]
        public async Task<IActionResult> SuspendSale([FromBody] List<CartDto> cart)
        {
            if (IsViewer()) return Json(new { success = false, message = "Viewer cannot do this - view only!" });
            try
            {
                if (cart == null || !cart.Any()) return Json(new { success = false, message = "Cart empty" });
                var suspended = new SuspendedSale { SuspendedNo = "SUS-" + DateTime.Now.ToString("yyyyMMddHHmmss"), DateSuspended = DateTime.Now, TotalAmount = 0, Items = new List<SuspendedSaleItem>() };
                decimal total = 0;
                foreach (var c in cart)
                {
                    var p = await _context.Products.FindAsync(c.ProductId);
                    if (p == null) continue;
                    decimal unitPrice = c.Price > 0 ? c.Price : (p.RetailPrice > 0 ? p.RetailPrice : p.SellingPrice);
                    if (unitPrice == 0) unitPrice = p.SellingPrice;
                    total += unitPrice * c.Quantity;
                    suspended.Items.Add(new SuspendedSaleItem { ProductId = p.Id, ProductName = p.Name, Quantity = c.Quantity, Price = unitPrice });
                }
                suspended.TotalAmount = total;
                _context.SuspendedSales.Add(suspended);
                await _context.SaveChangesAsync();
                return Json(new { success = true, id = suspended.Id });
            }
            catch (Exception ex)
            {
                var msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return Json(new { success = false, message = msg });
            }
        }
        public async Task<IActionResult> GetSuspendedSales() { var suspended = await _context.SuspendedSales.Include(s => s.Items).OrderByDescending(s => s.DateSuspended).Select(s => new { Id = s.Id, SuspendedNo = s.SuspendedNo, TotalAmount = s.TotalAmount, DateSuspended = s.DateSuspended, ItemsCount = s.Items.Count, Items = s.Items.Select(i => new { i.ProductId, i.ProductName, i.Quantity, i.Price }) }).ToListAsync(); return Json(suspended); }
        [HttpPost]
        public async Task<IActionResult> ResumeSale(int id) { var s = await _context.SuspendedSales.Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id); if (s == null) return Json(new { success = false }); var cartJson = System.Text.Json.JsonSerializer.Serialize(s.Items.Select(i => new { ProductId = i.ProductId, name = i.ProductName, quantity = i.Quantity, price = i.Price })); _context.SuspendedSales.Remove(s); await _context.SaveChangesAsync(); return Json(new { cart = cartJson }); }

        public async Task<IActionResult> Debtors()
        {
            // FIX 5: DEBTORS NEEDS LOGIN - ADDED
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("Username")))
                return RedirectToAction("Login", "Auth");

            var debtors = await _context.Sales.Where(s => s.Balance > 0).OrderByDescending(s => s.Date).ToListAsync();
            ViewBag.Role = HttpContext.Session.GetString("Role"); // ADDED
            return View(debtors);
        }

        [HttpPost]
        public async Task<IActionResult> PayDebt(int saleId, decimal amount)
        {
            if (IsViewer()) return Json(new { success = false, message = "Viewer cannot do this - view only!" });
            var sale = await _context.Sales.FindAsync(saleId);
            if (sale == null) return Json(new { success = false });
            sale.AmountPaid += amount;
            sale.Balance = sale.TotalAmount - sale.AmountPaid;
            if (sale.Balance < 0) sale.Balance = 0;
            sale.IsFullyPaid = sale.Balance <= 0;
            await _context.SaveChangesAsync();
            return Json(new { success = true, balance = sale.Balance });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateDebtorInfo(int saleId, string customerName, string customerPhone)
        {
            if (IsViewer()) return Json(new { success = false, message = "Viewer cannot do this - view only!" });
            var sale = await _context.Sales.FindAsync(saleId);
            if (sale == null) return Json(new { success = false });
            sale.CustomerName = customerName;
            sale.CustomerPhone = customerPhone;
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
    }

    public class CartDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; } // FIX: Added
        public string? PriceType { get; set; } // FIX: Added
    }
    public class CheckoutRequestDto
    {
        public List<CartDto> Items { get; set; } = new();
        public string PaymentType { get; set; } = "Cash";
        public decimal AmountPaid { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
    }
}
