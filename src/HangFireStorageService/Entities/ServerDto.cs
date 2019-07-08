using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HangFireStorageService.Dto
{
    public class ServerDtos
    {
        public string ServerId { get; set; }

        public string Data { get; set; }

        public DateTime? LastHeartbeat { get; set; }
    }
}
