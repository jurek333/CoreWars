using System;
using System.Collections.Generic;

namespace CoreWars.WebApi.Diagnostic
{
    public class Heartbeat
    {
        public Heartbeat()
        {
            this.Time = DateTime.UtcNow;
        }
        public Heartbeat(string desc):this()
        {
            this.Description = desc;
        }
        public DateTime Time { get; set; }
        public string Description { get; set; }
    }

    public class AppHeartbeat: Heartbeat
    {
        public AppHeartbeat():base() {
            this.AnotherModules = new List<AppHeartbeat>();
        }

        public AppHeartbeat(string desc) : base(desc)
        {
            this.AnotherModules = new List<AppHeartbeat>();
        }

        public List<AppHeartbeat> AnotherModules { get; set; }
    }
}