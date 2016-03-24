using Akka.Actor;
using Akka.Routing;
using Autofac;
using Autofac.Integration.WebApi;
using CoreWars.WebApi.Controllers;
using CoreWars.WebApi.Diagnostic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace CoreWars.WebApi.App_Start
{
    public class ClientSystem
    {
        private ActorSystem _system;
        private IActorRef _clusterAgent;

        public ClientSystem(string systemName)
        {
            _system = ActorSystem.Create(systemName);

            _clusterAgent = _system.ActorOf(Props.Empty.WithRouter(FromConfig.Instance), "health");
        }

        public IActorRef GetContactActor()
        {
            return _clusterAgent;
        }
    }

    public class AutofacConfig
    {
        private ClientSystem _system;

        public AutofacConfig(ClientSystem system)
        {
            _system = system;
        }

        public void Register(HttpConfiguration config)
        {
            var configuration = GlobalConfiguration.Configuration;

            var containerBuilder = new ContainerBuilder();

            containerBuilder.RegisterType<ValuesController>().InstancePerRequest();
            containerBuilder.RegisterType<HeartbeatController>().InstancePerRequest();
            containerBuilder.Register(cts => _system).As<ClientSystem>().SingleInstance();
            containerBuilder.Register(cts => _system.GetContactActor())
                .As<IActorRef>().SingleInstance();//.InstancePerDependency();


            var container = containerBuilder.Build();
            configuration.DependencyResolver = new AutofacWebApiDependencyResolver(container);
        }
    }
}