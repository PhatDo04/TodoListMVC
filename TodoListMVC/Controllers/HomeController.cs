using System.Web.Mvc;

namespace TodoListMVC.Controllers
{
    public class HomeController : Controller
    {
        [AllowAnonymous] // Trang Home không cần đăng nhập
        public ActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        [AllowAnonymous]
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}