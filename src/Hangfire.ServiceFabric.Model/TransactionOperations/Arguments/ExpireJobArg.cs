using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.ServiceFabric.Model.TransactionOperations.Arguments
{
    public class ExpireJobArg
    {
        public string JobId { get; set; }

        public TimeSpan ExpireIn { get; set; }
    }
}
