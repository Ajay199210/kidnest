using Microsoft.AspNetCore.Mvc;

namespace KidNest.Web.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/{statusCode}")]
        public IActionResult Index(int statusCode)
        {
            return statusCode switch
            {
                404 => View("NotFound"),
                _   => View("Generic")
            };
        }

        [Route("Error/Generic")]
        public IActionResult Generic()
        {
            return View();
        }
    }
}
