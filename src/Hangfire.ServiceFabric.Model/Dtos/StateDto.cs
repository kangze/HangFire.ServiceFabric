using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.ServiceFabric.Model.Dtos
{
    [DataContract]
    public class StateDto
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Reason { get; set; }

        [DataMember]
        public DateTime CreatedAt { get; set; }

        [DataMember]
        public Dictionary<string, string> Data { get; set; }
    }
}
