using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HangFireStorageService.Dto
{
    public class AggregatedCounterDto
    {
        public long Id { get; set; }

        public string Key { get; set; }

        public int Value { get; set; }

        public DateTime? ExpireAt { get; set; }
    }
}
