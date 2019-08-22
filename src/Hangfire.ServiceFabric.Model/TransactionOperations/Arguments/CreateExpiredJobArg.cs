using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire.ServiceFabric.Model.Dtos;

namespace Hangfire.ServiceFabric.Model.TransactionOperations.Arguments
{
    public class CreateExpiredJobArg
    {
        public JobDto JobDto { get; set; }
    }
}
