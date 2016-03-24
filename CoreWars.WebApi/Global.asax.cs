using CoreWars.Infrastructure.Messages;
using CoreWars.WebApi.App_Start;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;

namespace CoreWars.WebApi
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            string clusterName = ConfigurationManager.AppSettings["ClusterName"] ?? "CoreWarsCluster";
            AutofacConfig autofac = new AutofacConfig(new ClientSystem(clusterName));
            GlobalConfiguration.Configure(autofac.Register);            
        }
    }
}
