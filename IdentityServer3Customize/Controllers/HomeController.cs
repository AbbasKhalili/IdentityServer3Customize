using System.Threading.Tasks;
using System.Web.Mvc;

namespace IdentityServer3Customize.Controllers
{
    [Authorize]
    [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
    public class HomeController : Controller
    {
        [HandleError]
        public async Task<ActionResult> Index()
        {
            return View();
        }

    }
}