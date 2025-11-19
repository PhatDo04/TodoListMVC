using System.Web.Mvc;

namespace TodoListMVC.Controllers
{
    /// <summary>
    /// Controller xử lý các trang lỗi
    /// </summary>
    public class ErrorController : Controller
    {
        /// <summary>
        /// Hiển thị trang lỗi chung
        /// </summary>
        /// <param name="message">Thông báo lỗi</param>
        public ActionResult ShowError(string message)
        {
            ViewBag.ErrorMessage = message ?? "An unexpected error occurred.";
            return View();
        }

        /// <summary>
        /// Trang 404 - Not Found
        /// </summary>
        public ActionResult NotFound()
        {
            Response.StatusCode = 404;
            ViewBag.ErrorMessage = "The page you're looking for could not be found.";
            return View("ShowError");
        }

        /// <summary>
        /// Trang 500 - Internal Server Error
        /// </summary>
        public ActionResult InternalError()
        {
            Response.StatusCode = 500;
            ViewBag.ErrorMessage = "An internal server error occurred. Please try again later.";
            return View("ShowError");
        }
    }
}