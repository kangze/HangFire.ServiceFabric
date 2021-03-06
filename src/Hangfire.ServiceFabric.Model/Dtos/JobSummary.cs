﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.ServiceFabric.Model.Dtos
{
    [DataContract]
    public class JobSummary
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string InvocationData { get; set; }

        [DataMember]
        public string Arguments { get; set; }

        [DataMember]
        public DateTime CreatedAt { get; set; }

        [DataMember]
        public DateTime? ExpireAt { get; set; }

        [DataMember]
        public DateTime? FetchedAt { get; set; }

        [DataMember]
        public string StateName { get; set; }

        [DataMember]
        public string StateReason { get; set; }

        [DataMember]
        public Dictionary<string, string> StateData { get; set; }
    }
}
