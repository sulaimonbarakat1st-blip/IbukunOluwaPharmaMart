using Microsoft.AspNetCore.Mvc;
using IbukunOluwaPharmaMart.Data;
using IbukunOluwaPharmaMart.Models;
using Microsoft.EntityFrameworkCore;

namespace IbukunOluwaPharmaMart.Controllers
{
    public class AuthController : Controller
    {
        private readonly AppDbContext _context;
        public AuthController(AppDbContext context) => _context = context;

        public IActionResult Login()
        {
            // AUTO-CREATE DEFAULT ADMIN IF NO STAFF EXISTS
            if (!_context.Staffs.Any())
            {
                _context.Staffs.Add(new Staff
                {
                    Username = "admin",
                    Password = "admin123",
                    Role = "Admin",
                    FullName = "Main Admin"
                });
                _context.Staffs.Add(new Staff
                {
                    Username = "cashier1",
                    Password = "cashier123",
                    Role = "Cashier",
                    FullName = "Cashier One"
                });
                _context.SaveChanges();
            }
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var staff = await _context.Staffs.FirstOrDefaultAsync(s => s.Username == username && s.Password == password);
            if (staff == null)
            {
                ViewBag.Error = "Wrong username or password";
                return View();
            }

            HttpContext.Session.SetString("Username", staff.Username);
            HttpContext.Session.SetString("Role", staff.Role);
            HttpContext.Session.SetString("FullName", staff.FullName);

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        public async Task<IActionResult> ManageStaff()
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction("Login");

            var staffs = await _context.Staffs.ToListAsync();
            return View(staffs);
        }

        [HttpPost]
        public async Task<IActionResult> CreateStaff(string username, string password, string role, string fullname)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction("Login");

            var s = new Staff { Username = username, Password = password, Role = role, FullName = fullname };
            _context.Staffs.Add(s);
            await _context.SaveChangesAsync();
            return RedirectToAction("ManageStaff");
        }

        public async Task<IActionResult> DeleteStaff(int id)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction("Login");

            var s = await _context.Staffs.FindAsync(id);
            if (s != null) { _context.Staffs.Remove(s); await _context.SaveChangesAsync(); }
            return RedirectToAction("ManageStaff");
        }
    }
}