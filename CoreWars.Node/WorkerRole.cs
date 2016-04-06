using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Akka.Actor;
using System.Configuration;
using CoreWars.Infrastructure.Messages;
using Akka.Configuration.Hocon;
using Akka.Configuration;

namespace CoreWars.Node
{
    public class AppSettings
    {
        public static readonly string SystemName = ConfigurationManager.AppSettings["ClusterName"];

        private static int _lastIndex = 0;

        public static Config ConfigureAkka()
        {
            string[] ports = ConfigurationManager.AppSettings["ports"].Split(',');
            if (_lastIndex >= ports.Length)
                _lastIndex = 0;
            string port = ports[_lastIndex++];

            var section = (AkkaConfigurationSection)ConfigurationManager.GetSection("akka");
            var clusterConfig = section.AkkaConfig;

            var finalConfig = ConfigurationFactory.ParseString(
               string.Format("akka.remote.helios.tcp.port = {0}", port))
               .WithFallback(clusterConfig);

            return finalConfig;
        }
    }

    public class HealthActor : ReceiveActor
    {
        private Akka.Event.ILoggingAdapter _logger = Akka.Event.Logging.GetLogger(Context);

        public HealthActor()
        {
            Receive<HearthBeat>(msg =>
            {
                _logger.Info("HearthBeat ");
                Sender.Tell(new HearthBeat(Self.Path.Address.ToString() + string.Join("/", Self.Path.Elements), msg));
            });
        }

        protected override void PreStart()
        {
            _logger.Info("Actor {0} starting...", string.Join(",", Self.Path.Elements));
            base.PreStart();
        }

        protected override void PostStop()
        {
            base.PostStop();
            _logger.Info("Actor {0} stopped...", string.Join(",", Self.Path.Elements));
        }
    }

    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);

        private ActorSystem _system;

        public override void Run()
        {
            try
            {
                this.RunAsync(this.cancellationTokenSource.Token).Wait();
            }
            finally
            {
                this.runCompleteEvent.Set();
            }
        }

        public override bool OnStart()
        {
            ServicePointManager.DefaultConnectionLimit = 12;

            bool result = base.OnStart();
            string systemName = AppSettings.SystemName;

            _system = ActorSystem.Create(systemName, AppSettings.ConfigureAkka());
            IActorRef health = _system.ActorOf(Props.Create<HealthActor>(), "health");

            Trace.TraceInformation("CoreWars.Node has been started");

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("CoreWars.Node is stopping");

            var termination = _system.Terminate();

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Task.WaitAll(termination);

            Trace.TraceInformation("CoreWars.Node has stopped");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            await Task.WhenAll(_system.WhenTerminated);
        }
    }
}
