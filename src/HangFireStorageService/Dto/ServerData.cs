using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HangFireStorageService.Dto
{
    public class ServerData
    {
        public int WorkCount { get; set; }

        public string[] Queues { get; set; }

        public DateTime? StartedAt { get; set; }
    }
}
