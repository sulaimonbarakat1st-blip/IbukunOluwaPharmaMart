using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using IbukunOluwaPharmaMart.Data;
using IbukunOluwaPharmaMart.Models;

namespace IbukunOluwaPharmaMart.Controllers
{
    public class ProductsController : Controller
    {
        private readonly AppDbContext _context;
        public ProductsController(AppDbContext context) => _context = context;

        public async Task<IActionResult> Index(string search)
        {
            var query = _context.Products.Include(p => p.Category).AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Name.Contains(search) || (p.NAFDACNo != null && p.NAFDACNo.Contains(search)));
            }

            ViewBag.Search = search;
            return View(await query.OrderByDescending(p => p.Id).ToListAsync());
        }
        public IActionResult Create()
        {
            if (!_context.Categories.Any())
            {
                _context.Categories.AddRange(
                    new Category { Name = "Antimalarial" },
                    new Category { Name = "Antibiotics" },
                    new Category { Name = "Analgesics" },
                    new Category { Name = "Vitamins" },
                    new Category { Name = "Cough & Cold" },
                    new Category { Name = "Antiseptics" },
                    new Category { Name = "Blood Tonic"},
                    new Category { Name = "Weight Gain" },
                    new Category { Name = "anti Depressants" },
                    new Category { Name = "anti Diabetics" },
                    new Category { Name = "Anti Fungal" },
                    new Category { Name = "Anti Viral" },
                    new Category { Name = "Anti Ulcer" },
                    new Category { Name = "Diuretics" },
                    new Category { Name = "Corticosteroids" },
                    new Category { Name = "Sedatives" },
                    new Category { Name = "Bronchodilator" }
                );
                _context.SaveChanges();
            }
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                TempData["success"] = "Product added successfully!";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name");
            return View(product);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Update(product);
                await _context.SaveChangesAsync();
                TempData["success"] = "Product updated!";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var p = await _context.Products.FindAsync(id);
            if (p != null) { _context.Products.Remove(p); await _context.SaveChangesAsync(); }
            return RedirectToAction(nameof(Index));
        }
    }
}