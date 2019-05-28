using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HangFireStorageService.Dto
{
    public class JobQueueDto
    {
        public string Queue { get; set; }

        public long JobId { get; set; }

        public DateTime? FetchedAt { get; set; }
    }
}
