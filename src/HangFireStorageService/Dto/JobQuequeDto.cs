using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HangFireStorageService.Dto
{
    public class JobQuequeDto
    {
        public long JobId { get; set; }

        public string Queue { get; set; }

        public DateTime? FetchedAt { get; set; }
    }
}
