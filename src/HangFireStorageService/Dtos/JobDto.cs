using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Hangfire.ServiceFabric.Entities;

namespace Hangfire.ServiceFabric.Dtos
{
    [DataContract]
    public class JobDto : JobEntity
    {
        [DataMember]
        public string Reason { get; set; }

        [DataMember]
        public string StateData { get; set; }

        [DataMember]
        public DateTime? StateChanged { get; set; }
    }
}
