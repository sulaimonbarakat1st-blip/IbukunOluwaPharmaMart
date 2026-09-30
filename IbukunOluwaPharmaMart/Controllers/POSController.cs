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

        public async Task<IActionResult> Sell()
        {
            var products = await _context.Products.Include(p => p.Category).Where(p => p.Quantity > 0).ToListAsync();
            return View(products);
        }

        public async Task<IActionResult> Search(string term)
        {
            var products = await _context.Products
              .Where(p => p.Name.Contains(term) && p.Quantity > 0)
              .Select(p => new { p.Id, p.Name, p.SellingPrice, p.Quantity, p.Dosage })
              .Take(10).ToListAsync();
            return Json(products);
        }

        [HttpPost]
        public async Task<IActionResult> Checkout([FromBody] List<CartDto> cart)
        {
            if (cart == null || !cart.Any()) return BadRequest();

            var sale = new Sale
            {
                ReceiptNo = "REC-" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                Date = DateTime.Now,
                TotalAmount = 0
            };
            _context.Sales.Add(sale);
            await _context.SaveChangesAsync();

            decimal total = 0;
            foreach (var item in cart)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product == null || product.Quantity < item.Quantity) continue;

                product.Quantity -= item.Quantity;
                total += product.SellingPrice * item.Quantity;

                _context.SaleItems.Add(new SaleItem
                {
                    SaleId = sale.Id,
                    ProductId = product.Id,
                    Quantity = item.Quantity,
                    SellingPrice = product.SellingPrice
                });
            }
            sale.TotalAmount = total;
            await _context.SaveChangesAsync();
            return Json(new { success = true, receiptNo = sale.ReceiptNo });
        }
    }
    public class CartDto { public int ProductId { get; set; } public int Quantity { get; set; } }
}