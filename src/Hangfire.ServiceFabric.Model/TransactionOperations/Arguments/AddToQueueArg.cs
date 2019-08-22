using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.ServiceFabric.Model.TransactionOperations.Arguments
{
    public class AddToQueueArg
    {
        public string Id { get; set; }

        public string JobId { get; set; }

        public string Queue { get; set; }

    }
}
