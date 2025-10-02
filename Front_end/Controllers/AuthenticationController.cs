using Microsoft.AspNetCore.Mvc;

namespace Front_end.Controllers
{
    public class AuthenticationController : Controller
    {
        public IActionResult Register()
        {
            return View();
        }

        public IActionResult Verify()
        {
            return View();
        }
    }
}
