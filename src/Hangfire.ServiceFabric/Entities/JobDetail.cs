using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HangFireStorageService.Dto
{
    public class JobDetail
    {
        public string Id { get; set; }

        public long StateId { get; set; }

        public string StateName { get; set; }

        public string InvocationData { get; set; }

        public string Arguments { get; set; }

        public DateTime CreateAt { get; set; }

        public DateTime? ExpireAt { get; set; }

        public DateTime? FetchedAt { get; set; }

        public string Reason { get; set; }

        public string StateReason { get; set; }

        public string StateData { get; set; }

        public DateTime? StateChanged { get; set; }
    }
}
