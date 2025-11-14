using System.Web.Http;
using System.Web.Http.Cors;
using TodoListMVC.Handlers;

namespace TodoListMVC.App_Start
{
    public class WebApiConfig
    {
        private static bool _isConfigured = false; // ← THÊM FLAG

        public static void Register(HttpConfiguration config)
        {
            // ✅ Kiểm tra đã config chưa
            if (_isConfigured)
                return; // Thoát nếu đã config rồi

            _isConfigured = true; // Đánh dấu đã config

            // Add CORS handler to handle OPTIONS requests
            config.MessageHandlers.Add(new CorsHandler());

            // Enable CORS globally
            var cors = new EnableCorsAttribute(
                origins: "*",         // Cho phép tất cả origins
                headers: "*",         // Cho phép tất cả headers
                methods: "*"          // Cho phép tất cả methods (GET, POST, PUT, DELETE)
            );
            config.EnableCors(cors);

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