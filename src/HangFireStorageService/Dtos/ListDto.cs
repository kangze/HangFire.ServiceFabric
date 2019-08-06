using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.ServiceFabric.Dtos
{
    [DataContract]
    public class ListDto
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public DateTime? ExpireAt { get; set; }

        [DataMember]
        public string Item { get; set; }

        [DataMember]
        public string Value { get; set; }
    }
}
