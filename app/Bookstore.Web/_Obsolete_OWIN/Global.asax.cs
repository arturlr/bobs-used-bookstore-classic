// using System.Web; // Removed for .NET 8
using Microsoft.AspNetCore.Mvc;
// using System.Web.Optimization; // Removed for .NET 8
// using System.Web.Routing; // Removed for .NET 8
using NLog;

namespace Bookstore.Web
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_Error()
        {
            var ex = Server.GetLastError();
            var logger = LogManager.GetCurrentClassLogger();

            logger.Error(ex);
        }
    }
}
