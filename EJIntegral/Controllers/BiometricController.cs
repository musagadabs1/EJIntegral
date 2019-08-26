using System.Web.Mvc;

namespace EJIntegral.Controllers
{
    [Authorize]
    public class BiometricController : Controller
    {
        // GET: Biometric
        public ActionResult Index(string staffId)
        {
            return View();
        }

        //capture image
        public ActionResult ImageCapture(string staffId)
        {
            return View();
        }

       


    }
}
