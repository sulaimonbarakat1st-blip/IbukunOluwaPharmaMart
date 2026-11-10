using Microsoft.AspNetCore.Mvc;

namespace IbukunOluwaPharmaMart.Models
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
