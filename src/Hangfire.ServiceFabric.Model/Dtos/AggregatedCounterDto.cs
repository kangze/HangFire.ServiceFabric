using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.ServiceFabric.Model.Dtos
{
    [DataContract]
    public class AggregatedCounterDto
    {
        [DataMember]
        public long Id { get; set; }

        [DataMember]
        public string Key { get; set; }

        [DataMember]
        public int Value { get; set; }

        [DataMember]
        public DateTime? ExpireAt { get; set; }
    }
}
