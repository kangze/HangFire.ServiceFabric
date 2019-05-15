using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire.States;

namespace HangFireStorageService.Dto.Internal
{
    public class AddJobStateArgument
    {
        public string JobId { get; set; }

        public IState State { get; set; }
    }
}
