using System.Configuration;
using System.Web.Mvc;
using Enrollment_System.Controllers.Service;
using Unity;
using Unity.Injection;
using Unity.Mvc5;

namespace Enrollment_System
{
    public static class UnityConfig
    {
        public static void RegisterComponents()
        {
            var container = new UnityContainer();

            // Register services here
            container.RegisterType<BaseControllerServices>(new InjectionConstructor(ConfigurationManager.ConnectionStrings["DbConnection"].ConnectionString));

            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
        }
    }
}