using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.ServiceFabric.Dtos
{
    [DataContract]
    public class JobQueueDto
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Queue { get; set; }

        [DataMember]
        public string JobId { get; set; }

        [DataMember]
        public DateTime? FetchedAt { get; set; }
    }
}
