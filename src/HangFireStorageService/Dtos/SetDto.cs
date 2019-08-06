using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.ServiceFabric.Dtos
{
    [DataContract]
    public class SetDto
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Key { get; set; }

        [DataMember]
        public double Score { get; set; }

        [DataMember]
        public string Value { get; set; }

        [DataMember]
        public DateTime ExpireAt { get; set; }
    }
}
