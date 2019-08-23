using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.ServiceFabric.Model.TransactionOperations.Arguments
{
    public class SetRangInHashArg
    {
        public string Key { get; set; }

        public Dictionary<string, string> KeyValuePairs { get; set; }
    }
}
