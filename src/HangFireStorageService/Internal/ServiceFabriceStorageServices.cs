using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire.ServiceFabric.Servces;
using HangFireStorageService.Servces;

namespace Hangfire.ServiceFabric.Internal
{
    public class ServiceFabriceStorageServices : IServiceFabriceStorageServices
    {
        public IJobQueueAppService JobQueueAppService { get; }
        public IJobAppService JobAppService { get; }
        public IServerAppService ServerAppService { get; }
        public ICounterAppService CounterAppService { get; }
        public IAggregatedCounterAppService AggregatedCounterAppService { get; }
        public IJobSetAppService JobSetAppService { get; }
        public IHashAppService HashAppService { get; }
        public IListAppService ListAppService { get; }
        public IResourceLockAppService ResourceLockAppService { get; }

        public ServiceFabriceStorageServices(
            IJobQueueAppService jobQueueAppService,
            IJobAppService jobAppService,
            IServerAppService serverAppService,
            ICounterAppService counterAppService,
            IAggregatedCounterAppService aggregatedCounterAppService,
            IJobSetAppService jobSetAppService,
            IHashAppService hashAppService,
            IListAppService jobListAppService,
            IResourceLockAppService resourceLockAppService
            )
        {
            this.JobQueueAppService = jobQueueAppService;
            this.ServerAppService = serverAppService;
            this.CounterAppService = counterAppService;
            this.AggregatedCounterAppService = aggregatedCounterAppService;
            this.JobSetAppService = jobSetAppService;
            this.JobAppService = jobAppService;
            this.HashAppService = hashAppService;
            this.ListAppService = jobListAppService;
            this.ResourceLockAppService = resourceLockAppService;
        }
    }
}
