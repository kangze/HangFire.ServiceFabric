using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire.ServiceFabric.Model.Dtos;

namespace Hangfire.ServiceFabric.Model.TransactionOperations.Arguments
{
    public class AddJobStateArg : JobArg
    {

        public StateDto StateDto { get; set; }
    }
}
