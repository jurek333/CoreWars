using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreWars.Infrastructure.Messages
{
    public class HearthBeat
    {
        public string Coorelation { get; set; }
        public string ActorPath { get; set; }
        public DateTime Time { get; }

        public HearthBeat() : this(null, null)
        {
        }

        public HearthBeat(string patient, HearthBeat request)
        {
            this.Time = DateTime.UtcNow;

            if (null == request)
                this.Coorelation = new Guid().ToString();
            else
                this.Coorelation = request.Coorelation;

            if (!string.IsNullOrWhiteSpace(patient))
                this.ActorPath = patient;
        }
    }
}
