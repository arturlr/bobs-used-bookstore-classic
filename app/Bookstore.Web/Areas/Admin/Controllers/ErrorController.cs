using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.Web.Areas.Admin.Controllers
{
    [AllowAnonymous]
    public class ErrorController : AdminAreaControllerBase
    {
        [Route("/Error/Index/{code:int}")]
        public ActionResult Index(int code)
        {
            return View();
        }

        [Route("/error")]
        public ActionResult Support()
        {
            return View("~/Views/Error/Index.cshtml");
        }
    }
}
