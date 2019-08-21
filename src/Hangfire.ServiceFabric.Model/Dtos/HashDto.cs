using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.ServiceFabric.Model.Dtos
{
    [DataContract]
    public class HashDto
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Key { get; set; }

        [DataMember]
        public Dictionary<string, string> Fields { get; set; }

        [DataMember]
        public DateTime? ExpireAt { get; set; }
    }
}
