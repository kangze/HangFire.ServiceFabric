using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.ServiceFabric.Entities
{
    [DataContract]
    public class JobEntity
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string StateId { get; set; }

        [DataMember]
        public string StateName { get; set; }

        [DataMember]
        public string InvocationData { get; set; }

        [DataMember]
        public string Arguments { get; set; }

        [DataMember]
        public Dictionary<string, string> Parameters { get; set; }

        [DataMember]
        public DateTime CreatedAt { get; set; }

        [DataMember]
        public DateTime? ExpireAt { get; set; }
    }
}
