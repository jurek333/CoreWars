using Akka.Actor;
using CoreWars.Infrastructure.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace CoreWars.WebApi.Diagnostic
{
    public class HeartbeatController : ApiController
    {
        private static IActorRef _router { get; set; }

        public HeartbeatController(IActorRef router)
        {
            _router = router;
        }

        public async Task<AppHeartbeat> Get()
        {
            /*
            _router.Tell(new HearthBeat("WebApiApplication", null));
            return new AppHeartbeat("smth is alive");
            */
            
            var result = await _router.Ask<HearthBeat>(new HearthBeat("WebApiApplication", null));
            return new AppHeartbeat(string.Format("{0} is alive", result.ActorPath));
            
        }
    }
}
