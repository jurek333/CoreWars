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

namespace CoreWars.Node
{
    public class AppSettings
    {
        public static readonly string SystemName = "ClusterName";
    }

    public class HealthActor : ReceiveActor
    {
        private Akka.Event.ILoggingAdapter _logger = Akka.Event.Logging.GetLogger(Context);

        public HealthActor()
        {
            Receive<HearthBeat>(msg =>
            {
                _logger.Info("HearthBeat ");
                Sender.Tell(new HearthBeat(Self.Path.Address.ToString() + "/user/" + Self.Path.Name, msg));
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
            string systemName = ConfigurationManager.AppSettings[AppSettings.SystemName];
            _system = ActorSystem.Create(systemName);
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
