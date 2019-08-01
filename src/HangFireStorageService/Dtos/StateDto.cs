using HangFireStorageService.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.ServiceFabric.Dtos
{
    [DataContract]
    public class StateDto
    {

        public string Name { get; set; }


        public string Reason { get; set; }


        public DateTime CreatedAt { get; set; }


        public Dictionary<string, string> Data { get; set; }
    }
}
