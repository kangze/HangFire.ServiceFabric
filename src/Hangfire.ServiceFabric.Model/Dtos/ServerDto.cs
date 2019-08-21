using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.ServiceFabric.Model.Dtos
{
    [DataContract]
    public class ServerDtos
    {
        [DataMember]
        public string ServerId { get; set; }

        [DataMember]
        public string Data { get; set; }

        [DataMember]
        public DateTime? LastHeartbeat { get; set; }
    }
}
