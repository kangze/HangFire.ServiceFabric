using Hangfire.ServiceFabric.Servces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.ServiceFabric.Internal
{
    public interface IServiceFabriceStorageServices
    {
        IJobQueueAppService JobQueueAppService { get; }
        IJobAppService JobAppService { get; }
        IServerAppService ServerAppService { get; }
        ICounterAppService CounterAppService { get; }
        IAggregatedCounterAppService AggregatedCounterAppService { get; }
        IJobSetAppService JobSetAppService { get; }
        IHashAppService HashAppService { get; }
        IListAppService ListAppService { get; }
        IResourceLockAppService ResourceLockAppService { get; }
    }
}
