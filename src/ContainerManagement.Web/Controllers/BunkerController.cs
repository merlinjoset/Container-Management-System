using Microsoft.AspNetCore.Mvc;

namespace ContainerManagement.Web.Controllers
{
    [Route("bunker")]
    public class BunkerController : Controller
    {
        [HttpGet("")]
        [HttpGet("dashboard")]
        public IActionResult Dashboard()
        {
            return View();
        }

        [HttpGet("planning")]
        public IActionResult Planning()
        {
            return View();
        }

        [HttpGet("review")]
        public IActionResult Review()
        {
            return View();
        }

        [HttpGet("prenominations")]
        public IActionResult Prenominations()
        {
            return View();
        }
    }
}
