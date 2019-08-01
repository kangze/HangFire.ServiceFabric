using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Hangfire.ServiceFabric.Entities;

namespace Hangfire.ServiceFabric.Dtos
{
    [DataContract]
    public class JobDto
    {
        public string Id { get; set; }

        public DateTime? ExpireAt { get; set; }

        public string StateName { get; set; }


        public string InvocationData { get; set; }


        public string Arguments { get; set; }


        public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();

        public List<StateDto> StateHistory { get; set; } = new List<StateDto>();

        public DateTime CreatedAt { get; set; }
    }
}
