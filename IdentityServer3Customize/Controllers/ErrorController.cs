using System.Web.Mvc;

namespace IdentityServer3Customize.Controllers
{
    [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
    public class ErrorController : Controller
    {
        public ActionResult Index()
        {
            if (Request.QueryString.Count > 0) return RedirectToAction("Index");
            return View();
        }

        public ActionResult NotFound()
        {
            if (Request.QueryString.Count > 0) return RedirectToAction("NotFound");
            return View();
        }
        public ActionResult Unauthorize()
        {
            if (Request.QueryString.Count > 0) return RedirectToAction("Unauthorize");
            return View();
        }
        public ActionResult Badrequest()
        {
            if (Request.QueryString.Count > 0) return RedirectToAction("Badrequest");
            return View();
        }
        public ActionResult Servererror()
        {
            if (Request.QueryString.Count > 0) return RedirectToAction("Servererror");
            return View();
        }
    }
}