using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Jwt;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using System;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using TodoListMVC.App_Start;

[assembly: OwinStartup(typeof(TodoListMVC.Startup))]

namespace TodoListMVC
{
    public class Startup
    {
        // Lấy giá trị từ file Web.config
        // Ví dụ: <add key="oidc:ClientId" value="your-client-id" />
        private static string clientId = ConfigurationManager.AppSettings["oidc:ClientId"];

        // Ví dụ: <add key="oidc:Authority" value="https://login.microsoftonline.com/your-tenant-id/v2.0" />
        private static string authority = ConfigurationManager.AppSettings["oidc:Authority"];

        // Ví dụ: <add key="oidc:RedirectUri" value="https://localhost:44300/" />
        private static string redirectUri = ConfigurationManager.AppSettings["oidc:RedirectUri"];
        public void Configuration(IAppBuilder app)
        {
            // CORS được xử lý bởi Web API EnableCorsAttribute trong WebApiConfig.cs
            // Không dùng app.UseCors ở đây để tránh duplicate headers

            // Cấu hình SSO với OpenID Connect
            // 1. Cấu hình loại xác thực "Mặc định"
            // Khi đăng nhập thành công, thông tin người dùng sẽ được lưu vào một cookie.
            // Dòng này báo cho ứng dụng biết phải dùng loại cookie nào.
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            // 2. Kích hoạt xác thực bằng Cookie
            // Đây là middleware sẽ tạo và đọc cookie đăng nhập.
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = CookieAuthenticationDefaults.AuthenticationType,
                LoginPath = new PathString("/Account/Login"), // Trang login khi chưa xác thực
                ExpireTimeSpan = TimeSpan.FromHours(1), // Cookie hết hạn sau 1 giờ
                SlidingExpiration = true, // Renew cookie khi user còn hoạt động
                CookieName = "TodoListMVC.Auth", // Tên cookie
                CookieHttpOnly = true, // Không cho JavaScript truy cập (bảo mật)
                CookieSecure = CookieSecureOption.SameAsRequest // HTTPS trong production
            });

            // 3. Kích hoạt xác thực OpenID Connect
            // Đây là middleware chính xử lý việc nói chuyện với nhà cung cấp SSO.
            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
            {
                RequireHttpsMetadata = false,
                // -- Thông tin cơ bản --
                ClientId = clientId,     // ID của ứng dụng (lấy từ nhà cung cấp)
                Authority = authority,   // Địa chỉ máy chủ SSO (lấy từ nhà cung cấp)
                RedirectUri = redirectUri, // Trang nhà cung cấp sẽ trả về sau khi đăng nhập
                PostLogoutRedirectUri = redirectUri, // Trang trả về sau khi logout

                // -- Cấu hình luồng (flow) --
                // Yêu cầu cả 'code' (cho phép lấy token ở back-end) và 'id_token' (thông tin người dùng)
                ResponseType = OpenIdConnectResponseType.CodeIdToken,
                Scope = OpenIdConnectScope.OpenIdProfile + " " + OpenIdConnectScope.Email, // Yêu cầu thông tin cơ bản

                // -- Xử lý sau khi đăng nhập --
                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    // Dùng để xử lý việc chuyển hướng trước khi đến identity provider
                    RedirectToIdentityProvider = notification =>
                    {
                        // Nếu không phải là signin request (có thể là signout), không làm gì
                        if (notification.ProtocolMessage.RequestType == OpenIdConnectRequestType.Logout)
                        {
                            // Logout - construct proper Auth0 logout URL
                            var logoutUri = $"{authority}v2/logout?client_id={clientId}";
                            
                            // Add return URL
                            var postLogoutUri = notification.Request.Scheme + "://" + notification.Request.Host + notification.Request.PathBase;
                            logoutUri += $"&returnTo={System.Uri.EscapeDataString(postLogoutUri)}";
                            
                            notification.Response.Redirect(logoutUri);
                            notification.HandleResponse();
                        }
                        else
                        {
                            // Signin request - đảm bảo redirect URI được set đúng
                            if (string.IsNullOrEmpty(notification.ProtocolMessage.RedirectUri))
                            {
                                notification.ProtocolMessage.RedirectUri = redirectUri;
                            }
                        }

                        return Task.FromResult(0);
                    },

                    // Xảy ra khi xác thực thất bại
                    AuthenticationFailed = context =>
                    {
                        context.HandleResponse();
                        context.Response.Redirect("/Error/ShowError?message=" + System.Uri.EscapeDataString(context.Exception.Message));
                        return Task.FromResult(0);
                    }
                }
            });

            // Cấu hình JWT Bearer
            ConfigureJWT(app);

            // Web API - chỉ xử lý requests bắt đầu bằng /api/
            app.Map("/api", api =>
            {
                var config = new HttpConfiguration();
                WebApiConfig.Register(config);
                api.UseWebApi(config);
            });
        }

        private void ConfigureJWT(IAppBuilder app)
        {
            var key = Encoding.UTF8.GetBytes(JwtConfig.Secret); // Thay thế bằng secret của bạn

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = JwtConfig.Issuer,

                ValidateAudience = true,
                ValidAudience = JwtConfig.Audience,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),

                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(2)
            };

            app.UseJwtBearerAuthentication(new JwtBearerAuthenticationOptions
            {
                AuthenticationMode = AuthenticationMode.Active,
                TokenValidationParameters = tokenValidationParameters
            });

        }
    }
}
