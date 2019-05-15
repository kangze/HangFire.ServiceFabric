using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HangFireStorageService.Dto
{
    public class JobDto
    {
        public long Id { get; set; }

        public long StateId { get; set; }

        public string StateName { get; set; }

        public string InvocationData { get; set; }

        public string Arguments { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? ExpireAt { get; set; }
    }
}
