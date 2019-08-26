using EJIntegral.Models;
using System.Linq;
using System.Web.Mvc;

namespace EJIntegral.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private EJIntegralDBEntities db = new EJIntegralDBEntities();
        public ActionResult Index(string staffId)
        {
            return View(db.GetEmployeeInfoWithPhoto(staffId).ToList());
            //return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public void Dispose()
        {
            db.Dispose();
        }
    }
}