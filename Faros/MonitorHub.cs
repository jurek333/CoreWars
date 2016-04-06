using Akka.Actor;
using Akka.Cluster;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Faros
{
    public class ClusterRoleLeader
    {
        public ClusterRoleLeader(string role, Address address)
        {
            Role = role;
            Address = address;
        }
        public string Role { get; private set; }
        public Address Address { get; private set; }
    }

    public class MonitorHub : Hub
    {
        public void ClusterState(ClusterEvent.CurrentClusterState clusterState, Address currentClusterAddress)
        {
            List<ClusterRoleLeader> clusterRoleLeaders = new List<ClusterRoleLeader>();
            foreach (var member in clusterState.Members)
            {
                foreach (var role in member.Roles)
                {
                    var address = clusterState.RoleLeader(role);
                    clusterRoleLeaders.Add(new ClusterRoleLeader(role, address));
                }
            }

            var context = GlobalHost.ConnectionManager.GetHubContext<MonitorHub>();
            context.Clients.All.broadcastClusterState(new
            {
                state = clusterState,
                leaders = clusterRoleLeaders,
                address = currentClusterAddress,
                time = DateTime.UtcNow
            });
        }
    }
}