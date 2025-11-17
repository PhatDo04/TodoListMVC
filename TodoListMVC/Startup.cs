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
            // Cors: cho phép tất cả origin - dev, production thì cấu hình cụ thể
            app.UseCors(CorsOptions.AllowAll);

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
                LoginPath = new PathString("/Account/Login") // Tùy chọn: Trang sẽ đến nếu chưa đăng nhập
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

                // -- Cấu hình luồng (flow) --
                // Yêu cầu cả 'code' (cho phép lấy token ở back-end) và 'id_token' (thông tin người dùng)
                ResponseType = OpenIdConnectResponseType.CodeIdToken,
                Scope = OpenIdConnectScope.OpenIdProfile + " " + OpenIdConnectScope.Email, // Yêu cầu thông tin cơ bản

                // -- Xử lý sau khi đăng nhập --
                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    // Dùng để xử lý việc chuyển hướng sau khi đăng nhập
                    RedirectToIdentityProvider = notification =>
                    {
                        // Nếu yêu cầu đăng nhập đến từ một trang cụ thể,
                        // lưu lại trang đó để quay về sau khi đăng nhập thành công.
                        if (notification.ProtocolMessage.RedirectUri == null)
                        {
                            notification.ProtocolMessage.RedirectUri = redirectUri;
                        }

                        // Giữ lại URL mà người dùng muốn truy cập ban đầu
                        string appBaseUrl = notification.Request.Scheme + "://" + notification.Request.Host + notification.Request.PathBase;
                        notification.ProtocolMessage.RedirectUri = appBaseUrl;

                        return Task.FromResult(0);
                    },

                    // Xảy ra khi xác thực thất bại
                    AuthenticationFailed = context =>
                    {
                        context.HandleResponse();
                        context.Response.Redirect("/Error/ShowError?message=" + context.Exception.Message);
                        return Task.FromResult(0);
                    }
                }
            });

            // Cấu hình JWT Bearer
            ConfigureJWT(app);
            // Web API
            var config = GlobalConfiguration.Configuration;
            WebApiConfig.Register(config);
            app.UseWebApi(config);
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
