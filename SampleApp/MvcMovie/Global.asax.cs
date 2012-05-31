using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace MvcMovie
{

    using MongoDB.Bson;

    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Movies", action = "About", id = UrlParameter.Optional } // Parameter defaults
            );

        }

        protected void Application_Start()
        {
            ModelBinders.Binders.Add(typeof(ObjectId), new ObjectIdBinder());
            RegisterRoutes(RouteTable.Routes);
        }


        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute
            {
                ExceptionType = typeof(MongoDB.Driver.MongoConnectionException),
                View = "ConnectError",
                Order = 2
            });
            filters.Add(new HandleErrorAttribute
            {
                ExceptionType = typeof(System.Net.Sockets.SocketException),
                View = "ConnectError",
                Order = 2
            });
            filters.Add(new HandleErrorAttribute());
        }
    }
}