using Microsoft.Owin;               
using Microsoft.Owin.Cors;
using Owin;
using System;

using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Jwt;
using System.Text;
using System.Web.Http;
using TodoListMVC.App_Start;

[assembly: OwinStartup(typeof(TodoListMVC.Startup))]

namespace TodoListMVC
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Cors: cho phép tất cả origin - dev, production thì cấu hình cụ thể
            app.UseCors(CorsOptions.AllowAll);  
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
