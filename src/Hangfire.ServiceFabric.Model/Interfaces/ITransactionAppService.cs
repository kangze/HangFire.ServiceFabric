using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire.ServiceFabric.Model.TransactionOperations;
using Microsoft.ServiceFabric.Services.Remoting;

namespace Hangfire.ServiceFabric.Model.Interfaces
{
    public interface ITransactionAppService : IService
    {
        Task CommitAsync(List<Operation> operations);
    }
}
