using System.Web;
using System.Web.Mvc;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;

namespace TodoListMVC.Controllers
{
    /// <summary>
    /// Controller xử lý authentication với SSO (OpenID Connect + Auth0)
    /// </summary>
    public class AccountController : Controller
    {
        /// <summary>
        /// Kích hoạt OpenID Connect login flow
        /// Redirect user đến Auth0 login page
        /// </summary>
        /// <param name="returnUrl">URL để quay về sau khi login thành công</param>
        public ActionResult Login(string returnUrl)
        {
            // Nếu user đã đăng nhập rồi, redirect về trang Tasks
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Tasks");
            }

            // Trigger OpenID Connect authentication middleware
            // Middleware sẽ redirect user đến Auth0
            HttpContext.GetOwinContext().Authentication.Challenge(
            new AuthenticationProperties { RedirectUri = returnUrl ?? "/" },
        OpenIdConnectAuthenticationDefaults.AuthenticationType
                );

            return new HttpUnauthorizedResult();
        }

        /// <summary>
        /// Đăng xuất khỏi ứng dụng và SSO provider (Auth0)
        /// </summary>
        public ActionResult Logout()
        {
            // Clear local authentication cookie first
            HttpContext.GetOwinContext().Authentication.SignOut(
                CookieAuthenticationDefaults.AuthenticationType
            );

            // Trigger OpenID Connect logout (this will redirect to Auth0 logout)
            // Auth0 will then redirect back to PostLogoutRedirectUri configured in Startup.cs
            HttpContext.GetOwinContext().Authentication.SignOut(
                OpenIdConnectAuthenticationDefaults.AuthenticationType
     );

            // This return won't execute immediately - the middleware will intercept
            // But we return it anyway for clarity
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Action được gọi sau khi logout thành công từ Auth0
        /// </summary>
        [AllowAnonymous]
        public ActionResult PostLogout()
        {
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Hiển thị user profile từ SSO claims
        /// Trang này yêu cầu authentication
        /// </summary>
        [Authorize]
        public ActionResult Profile()
        {
            // Lấy tất cả claims từ authenticated user
            var claims = ((System.Security.Claims.ClaimsPrincipal)User).Claims;

            // Truyền claims xuống view để hiển thị
            return View(claims);
        }

        /// <summary>
        /// Action được Auth0 callback sau khi login thành công
        /// Không cần implement vì OpenID Connect middleware tự xử lý
        /// </summary>
        public ActionResult Callback()
        {
            return RedirectToAction("Index", "Tasks");
        }

        /// <summary>
        /// Trang hiển thị khi có lỗi authentication
        /// </summary>
        public ActionResult AccessDenied()
        {
            ViewBag.Message = "Bạn không có quyền truy cập trang này.";
            return View();
        }
    }
}