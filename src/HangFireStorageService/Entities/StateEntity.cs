using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace HangFireStorageService.Dto
{
    [DataContract]
    public class StateEntity
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string JobId { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Reason { get; set; }

        [DataMember]
        public DateTime CreatedAt { get; set; }

        [DataMember]
        public string Data { get; set; }
    }
}
