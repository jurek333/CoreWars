using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using System.Web.Http;
using Owin;
using Microsoft.Owin.Hosting;
using System.Net;
using Akka.Actor;
using System.Threading.Tasks;
using System.Configuration;
using Autofac;
using System.Net.Http;

namespace Faros
{
    public class WebRole : RoleEntryPoint
    {
        private IDisposable _app = null;
        private ActorSystem _system;
        private IActorRef _monitor;
        private bool _closingApp = false;

        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();

            var builder = new ContainerBuilder();
            builder.RegisterInstance(_monitor).As<IActorRef>().SingleInstance();
            builder.RegisterType<MemberController>();
            var container = builder.Build();
            app.UseAutofacMiddleware(container);

            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute("Default", "{controller}/{id}", new { id = RouteParameter.Optional });
            app.UseWebApi(config);
        }

        public override bool OnStart()
        {
            ServicePointManager.DefaultConnectionLimit = 12;
            string clusterName = ConfigurationManager.AppSettings["ClusterName"];
            _system = ActorSystem.Create(clusterName);
            _monitor = _system.ActorOf(Props.Create<ClusterMonitor>(), "faros");

            var endpoint = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["Endpoint1"];
            string baseUri = String.Format("{0}://{1}", endpoint.Protocol, endpoint.IPEndpoint);
            _app = WebApp.Start(baseUri, this.Configuration);

            return base.OnStart();
        }

        public override void Run()
        {
            Task.WaitAll(_system.WhenTerminated);
            _closingApp = true;
        }

        public override void OnStop()
        {
            if (null != _app)
                _app.Dispose();

            if (!_closingApp)
            {
                _closingApp = true;
                _system.Terminate();
            }

            base.OnStop();
        }
    }

    public class MemberController : ApiController
    {
        private IActorRef _monitor;
        public MemberController(IActorRef monitor)
        {
            _monitor = monitor;
        }

        public HttpResponseMessage Get()
        {
            string address = _monitor.Path.Address.ToString();

            return new HttpResponseMessage()
            {
                Content = new StringContent(address)
            };
        }
        
    }

}
