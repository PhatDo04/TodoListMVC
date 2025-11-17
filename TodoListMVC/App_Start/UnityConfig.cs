using AutoMapper;
using System.Web.Http;
using TodoListMVC.App_Start;
using TodoListMVC.Repositories;
using Unity;
using Unity.Lifetime;
using Unity.WebApi;
namespace TodoListMVC
{
    public static class UnityConfig
    {
        public static void RegisterComponents()
        {
            var container = new UnityContainer();

            // Cấu hình AutoMapper, đọc từ MappingProfile
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });

            //Tạo người phiên dịch AutoMapper (IMapper)
            IMapper mapper = mapperConfig.CreateMapper();
            //Đăng ký IMapper với Unity Container (RegisterInstance để đảm bảo có 1 người duy nhất)
            container.RegisterInstance<IMapper>(mapper);

            // register all your components with the container here
            // it is NOT necessary to register your controllers

            // e.g. container.RegisterType<ITestService, TestService>();
            //Đăng ký IUnitOfWork với Unity Container (theo kiểu HierarchicalLifetimeManager)
            //HierarchicalLifetimeManager: mỗi yêu cầu HTTP sẽ có 1 thể hiện riêng biệt của UnitOfWork
            container.RegisterType<IUnitOfWork, UnitOfWork>(new HierarchicalLifetimeManager());
            GlobalConfiguration.Configuration.DependencyResolver = new UnityDependencyResolver(container);
        }
    }
}