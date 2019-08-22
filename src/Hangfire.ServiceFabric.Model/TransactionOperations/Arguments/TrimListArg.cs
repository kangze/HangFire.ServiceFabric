using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.ServiceFabric.Model.TransactionOperations.Arguments
{
    public class TrimListArg
    {
        public string Key { get; set; }

        public int KeepStartingFrom { get; set; }

        public int KeepEndingAt { get; set; }
    }
}
