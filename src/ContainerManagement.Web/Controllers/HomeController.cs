using Microsoft.AspNetCore.Mvc;

namespace ContainerManagement.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // Authenticated users land on the Bunker Pro dashboard
            if (User?.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Dashboard", "Bunker");
            }
            return View();
        }
    }
}
