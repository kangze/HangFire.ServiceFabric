using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HangFireStorageService.Dto
{
    public class StateDto
    {
        public long Id { get; set; }

        public long JobId { get; set; }

        public string Name { get; set; }

        public string Reason { get; set; }

        public DateTime CreatedAt { get; set; }

        public string Data { get; set; }
    }
}
