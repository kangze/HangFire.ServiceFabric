using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Storage;
using HangFireStorageService.Extensions;
using HangFireStorageService.Servces;

namespace HangFireStorageService.Internal
{
    public class ServiceFabricStorage : JobStorage
    {
        private readonly IJobQueueAppService _jobQueueAppService;
        private readonly IJobAppService _jobAppService;
        private readonly IServerAppService _serverAppService;
        private readonly ICounterAppService _counterAppService;
        private readonly IAggregatedCounterAppService _aggregatedCounterAppService;
        private readonly IJobSetAppService _jobSetAppService;
        private readonly IHashAppService _hashAppService;
        private readonly IListAppService _jobListAppService;

        private ServiceFabricStorage(
            IJobQueueAppService jobQueueAppService,
            IJobAppService jobAppService,
            IServerAppService serverAppService,
            ICounterAppService counterAppService,
            IAggregatedCounterAppService aggregatedCounterAppService,
            IJobSetAppService jobSetAppService,
            IHashAppService hashAppService,
            IListAppService jobListAppService
            )
        {
            this._jobQueueAppService = jobQueueAppService;
            this._serverAppService = serverAppService;
            this._counterAppService = counterAppService;
            this._aggregatedCounterAppService = aggregatedCounterAppService;
            this._jobSetAppService = jobSetAppService;
            this._jobAppService = jobAppService;
            this._hashAppService = hashAppService;
            this._jobListAppService = jobListAppService;
        }

        internal static ServiceFabricStorage Create(ServiceFabricOptions option)
        {
            RemotingClient.ApplicationUri = option.ApplicationUri;
            var jobQueueAppService = RemotingClient.CreateJobQueueAppService();
            var jobAppService = RemotingClient.CreateJobAppService();
            var serverAppService = RemotingClient.CreateServiceAppService();
            var counterAppService = RemotingClient.CreateCounterAppService();
            var jobSetAppService = RemotingClient.CreateJobSetAppService();
            var hashAppService = RemotingClient.CreateHashAppService();
            var aggregatedAppService = RemotingClient.CreateAggregateCounterAppService();
            var jobListAppService = RemotingClient.CreateJobListAppService();
            return new ServiceFabricStorage(
                jobQueueAppService,
                jobAppService,
                serverAppService,
                counterAppService,
                aggregatedAppService,
                jobSetAppService,
                hashAppService,
                jobListAppService);
        }


        public override IMonitoringApi GetMonitoringApi()
        {
            return new ServiceFabricMonitoringApi(this._jobQueueAppService, this._jobAppService, this._serverAppService, this._counterAppService, this._aggregatedCounterAppService, this._jobSetAppService);
        }

        public override IStorageConnection GetConnection()
        {
            return new ServiceFabricStorageConnect(this._jobAppService, this._serverAppService, this._jobSetAppService, this._hashAppService, this._jobQueueAppService, this._counterAppService, this._aggregatedCounterAppService, this._jobListAppService);
        }
    }
}
