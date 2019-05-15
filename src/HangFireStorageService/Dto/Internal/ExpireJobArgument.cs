using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HangFireStorageService.Dto.Internal
{
    public class ExpireJobArgument
    {
        public string JobId { get; set; }

        public TimeSpan ExpireIn { get; set; }
    }
}
