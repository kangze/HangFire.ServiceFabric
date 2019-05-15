using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire.Storage;

namespace HangFireStorageService.Internal
{
    public class ServiceFabricTransactionJob: IFetchedJob
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void RemoveFromQueue()
        {
            throw new NotImplementedException();
        }

        public void Requeue()
        {
            throw new NotImplementedException();
        }

        public string JobId { get; }
    }
}
