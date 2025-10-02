using Microsoft.AspNetCore.Mvc;

namespace Front_end.Controllers
{
    public class SubjectController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
