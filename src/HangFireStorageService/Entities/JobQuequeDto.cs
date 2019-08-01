using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HangFireStorageService.Dto
{
    public class JobQueueDto
    {
        public string Id { get; set; }

        public string Queue { get; set; }

        public string JobId { get; set; }

        public DateTime? FetchedAt { get; set; }
    }
}
