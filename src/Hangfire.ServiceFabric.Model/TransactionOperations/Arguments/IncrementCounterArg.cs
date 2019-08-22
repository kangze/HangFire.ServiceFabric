using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.ServiceFabric.Model.TransactionOperations.Arguments
{
    public class IncrementCounterArg
    {
        public string Key { get; set; }

        public int Value { get; set; }

        public TimeSpan? ExpireIn { get; set; }
    }
}
