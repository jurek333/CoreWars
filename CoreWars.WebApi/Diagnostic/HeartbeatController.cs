using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace CoreWars.WebApi.Diagnostic
{
    public class HeartbeatController : ApiController
    {
        public AppHeartbeat Get()
        {
            return new AppHeartbeat("Heartbeat controller is alive.");
        }
    }
}
