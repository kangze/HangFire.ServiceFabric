using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.ServiceFabric.Model.Dtos
{
    [DataContract]
    public class JobDto
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public DateTime? ExpireAt { get; set; }

        [DataMember]
        public string StateName { get; set; }

        [DataMember]
        public string InvocationData { get; set; }

        [DataMember]
        public string Arguments { get; set; }

        [DataMember]
        public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();

        [DataMember]
        public List<StateDto> StateHistory { get; set; } = new List<StateDto>();

        [DataMember]
        public DateTime CreatedAt { get; set; }
    }
}
