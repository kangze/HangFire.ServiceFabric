using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.ServiceFabric.Model.Dtos
{
    [DataContract]
    public class ServerData
    {
        [DataMember]
        public int WorkCount { get; set; }

        [DataMember]
        public string[] Queues { get; set; }

        [DataMember]
        public DateTime? StartedAt { get; set; }
    }
}
