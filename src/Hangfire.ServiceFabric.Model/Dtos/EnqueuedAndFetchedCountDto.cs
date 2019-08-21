using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.ServiceFabric.Model.Dtos
{
    [DataContract]
    public class EnqueuedAndFetchedCountDto
    {
        [DataMember]
        public int? EnqueuedCount { get; set; }

        [DataMember]
        public int? FetchedCount { get; set; }
    }
}
