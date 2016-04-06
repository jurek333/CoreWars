using Akka.Actor;
using Akka.Cluster;
using Akka.Event;
using System;

namespace Faros
{
    internal class ClusterMonitor : ReceiveActor
    {
        #region Messages
        public class GetStatus { }
        public class SendState { }
        public class MemberAddress
        {
            public string Protocol { get; }
            public string System { get; }
            public string Host { get; }
            public int Port { get; }

            public MemberAddress(string protocol, string system, string host, int port)
            {
                Protocol = protocol;
                System = system;
                Host = host;
                Port = port;
            }

            public Address ConvertToAddress()
            {
                return new Address(Protocol, System, Host, Port);
            }
        }
        public class RemoveMe { }
        public class MemberLeave : MemberAddress
        {
            public MemberLeave(string protocol, string system, string host, int port) : base(protocol, system, host, port) { }
        }
        public class MemberDown : MemberAddress
        {
            public MemberDown(string protocol, string system, string host, int port) : base(protocol, system, host, port) { }
        }
        #endregion

        private Cluster _cluster;
        private ICancelable _cancelStatusUpdates;
        private readonly ILoggingAdapter _logger = Context.GetLogger();

        private void Ready()
        {
            Receive<SendState>(msg =>
            {
                _cluster.SendCurrentClusterState(Self);
            });
            Receive<ClusterEvent.CurrentClusterState>(state =>
            {
                //_hub.ClusterState(state, _cluster.SelfAddress);
            });
            Receive<RemoveMe>(msg => {
                _logger.Warning("[l] Monitor stopping; Issuing a Cluster.Leave() command for following address: {0}", _cluster.SelfAddress);
                _cluster.Leave(_cluster.SelfAddress);
            });
            Receive<MemberLeave>(msg => {
                Address add = new Address(msg.Protocol, msg.System, msg.Host, msg.Port);
                _logger.Warning("[l] Forcing Member to leave cluster: {0}", add.ToString());
                _cluster.Leave(add);
            });
            Receive<MemberDown>(msg => {
                Address add = new Address(msg.Protocol, msg.System, msg.Host, msg.Port);
                _logger.Warning("[l] Forcing the Member down: {0}", add.ToString());
                _cluster.Down(add);
            });
        }

        public ClusterMonitor() {
            Ready();   
        }

        protected override void PreStart()
        {
            _cluster = Cluster.Get(Context.System);
            _cancelStatusUpdates = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(2), Self, new SendState(), Self);

            base.PreStart();
        }
    }

    
}