using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;

namespace TodoListMVC.App_Start
{
    public class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Enable CORS globally
            var cors = new EnableCorsAttribute(
                origins: "*",           // Cho phép tất cả origins
                headers: "*",           // Cho phép tất cả headers
                methods: "*"            // Cho phép tất cả methods (GET, POST, PUT, DELETE)
            );
            config.EnableCors(cors);

            // Cấu hình và dịch vụ Web API

            // Cấu hình Tuyến đường (Route) của Web API
            config.MapHttpAttributeRoutes(); // Bật route kiểu [Route("...")]

            // Đây là "con đường" mặc định cho API
            // Nó sẽ map "GET api/tasksapi/5" đến hàm "GetTask(5)"
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            // Cấu hình để API trả về JSON (thay vì XML)
            var json = config.Formatters.JsonFormatter;
            config.Formatters.Remove(config.Formatters.XmlFormatter);
        }
    }
}