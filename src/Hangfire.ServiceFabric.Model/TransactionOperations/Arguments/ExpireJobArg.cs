using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.ServiceFabric.Model.TransactionOperations.Arguments
{
    public class ExpireJobArg: JobArg
    {

        public TimeSpan ExpireIn { get; set; }
    }
}
