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
        private readonly IJobStateDataAppService _jobStateDataAppService;
        private readonly IServerAppService _serverAppService;
        private readonly ICounterAppService _counterAppService;
        private readonly IAggregatedCounterAppService _aggregatedCounterAppService;
        private readonly IJobSetAppService _jobSetAppService;
        private readonly IJobDataService _jobDataService;
        private readonly IHashAppService _hashAppService;
        private readonly IJobListAppService _jobListAppService;

        private ServiceFabricStorage(
            IJobQueueAppService jobQueueAppService,
            IJobAppService jobAppService,
            IJobStateDataAppService jobStateDataAppService,
            IServerAppService serverAppService,
            ICounterAppService counterAppService,
            IAggregatedCounterAppService aggregatedCounterAppService,
            IJobSetAppService jobSetAppService,
            IJobDataService jobDataService,
            IHashAppService hashAppService,
            IJobListAppService jobListAppService
            )
        {
            this._jobQueueAppService = jobQueueAppService;
            this._serverAppService = serverAppService;
            this._counterAppService = counterAppService;
            this._aggregatedCounterAppService = aggregatedCounterAppService;
            this._jobSetAppService = jobSetAppService;
            this._jobStateDataAppService = jobStateDataAppService;
            this._jobAppService = jobAppService;
            this._jobDataService = jobDataService;
            this._hashAppService = hashAppService;
            this._jobListAppService = jobListAppService;
        }

        internal static ServiceFabricStorage Create(ServiceFabricOptions option)
        {
            RemotingClient.ApplicationUri = option.ApplicationUri;
            var jobQueueAppService = RemotingClient.CreateJobQueueAppService();
            var jobAppService = RemotingClient.CreateJobAppService();
            var jobStateDataAppService = RemotingClient.CreateJobStateDataAppService();
            var serverAppService = RemotingClient.CreateServiceAppService();
            var counterAppService = RemotingClient.CreateCounterAppService();
            var jobSetAppService = RemotingClient.CreateJobSetAppService();
            var jobDataAppService = RemotingClient.CreateJobDataService();
            var hashAppService = RemotingClient.CreateHashAppService();
            var aggregatedAppService = RemotingClient.CreateAggregateCounterAppService();
            var jobListAppService = RemotingClient.CreateJobListAppService();
            return new ServiceFabricStorage(
                jobQueueAppService,
                jobAppService,
                jobStateDataAppService,
                serverAppService,
                counterAppService,
                aggregatedAppService,
                jobSetAppService,
                jobDataAppService,
                hashAppService,
                jobListAppService);
        }


        public override IMonitoringApi GetMonitoringApi()
        {
            return new ServiceFabricMonitoringApi(this._jobQueueAppService, this._jobAppService, this._jobStateDataAppService, this._serverAppService, this._counterAppService, this._aggregatedCounterAppService, this._jobSetAppService);
        }

        public override IStorageConnection GetConnection()
        {
            return new ServiceFabricStorageConnect(this._jobDataService, this._jobAppService, this._jobStateDataAppService, this._serverAppService, this._jobSetAppService, this._hashAppService, this._jobQueueAppService, this._counterAppService, this._aggregatedCounterAppService, this._jobListAppService);
        }
    }
}
