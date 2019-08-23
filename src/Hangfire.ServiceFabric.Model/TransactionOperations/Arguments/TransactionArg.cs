using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.ServiceFabric.Model.TransactionOperations.Arguments
{
    public abstract class TransactionArg
    {

    }

    public class JobArg
    {
        public string JobId { get; set; }
    }
}
