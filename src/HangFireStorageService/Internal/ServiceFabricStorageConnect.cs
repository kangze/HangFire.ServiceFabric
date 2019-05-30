using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hangfire.Common;
using Hangfire.Server;
using Hangfire.Storage;
using HangFireStorageService.Dto;
using HangFireStorageService.Servces;
using Mcs.Common.BaseServices;

namespace HangFireStorageService.Internal
{
    internal class ServiceFabricStorageConnect : IStorageConnection
    {

        private readonly IJobDataService _jobDataService;
        private readonly IJobAppService _jobAppService;
        private readonly IJobQueueAppService _jobQueueAppService;
        private readonly IJobStateDataAppService _jobStateDataAppService;
        private readonly IServerAppService _serverAppService;
        private readonly IJobSetAppService _jobSetsAppService;
        private readonly IHashAppService _hashAppService;
        private readonly ICounterAppService _counterAppService;
        private readonly IAggregatedCounterAppService _aggregatedCounterAppService;
        private readonly IJobListAppService _jobListAppService;

        public ServiceFabricStorageConnect(
            IJobDataService jobDataService,
            IJobAppService jobAppService,
            IJobStateDataAppService jobStateDataAppService,
            IServerAppService serverAppService,
            IJobSetAppService setsAppService,
            IHashAppService hashAppService,
            IJobQueueAppService jobQueueAppService,
            ICounterAppService counterAppService,
            IAggregatedCounterAppService aggregatedCounterAppService,
            IJobListAppService jobListAppService
            )
        {
            this._aggregatedCounterAppService = aggregatedCounterAppService;
            this._counterAppService = counterAppService;
            this._jobDataService = jobDataService;
            this._jobAppService = jobAppService;
            this._jobStateDataAppService = jobStateDataAppService;
            this._serverAppService = serverAppService;
            this._jobSetsAppService = setsAppService;
            this._hashAppService = hashAppService;
            this._jobQueueAppService = jobQueueAppService;
            this._jobListAppService = jobListAppService;
        }

        public void Dispose()
        {

        }

        public IWriteOnlyTransaction CreateWriteTransaction()
        {
            return new ServiceFabricWriteOnlyTransaction(this._jobQueueAppService, this._jobAppService, this._jobStateDataAppService, this._serverAppService, this._counterAppService, this._aggregatedCounterAppService, this._jobSetsAppService, this._jobDataService, this._hashAppService, this._jobListAppService);
        }

        public IDisposable AcquireDistributedLock(string resource, TimeSpan timeout)
        {
            var locker = new ServicesFabricDistributedLock(null, resource, timeout);
            return locker.AcquireLock().GetAwaiter().GetResult();
        }

        public string CreateExpiredJob(Job job, IDictionary<string, string> parameters, DateTime createdAt, TimeSpan expireIn)
        {
            var invocationData = InvocationData.SerializeJob(job);
            var playload = invocationData.SerializePayload(true);
            var jobDto = new JobDto()
            {
                Id = 0,//ID需要后续处理一下,
                InvocationData = playload,
                Arguments = invocationData.Arguments,
                CreatedAt = createdAt,
                ExpireAt = createdAt.Add(expireIn),
                Parameters = new Dictionary<string, string>()
            };
            foreach (var pair in parameters)
                jobDto.Parameters.Add(pair.Key, pair.Value);
            jobDto = this._jobDataService.AddJobAsync(jobDto).GetAwaiter().GetResult();
            return jobDto.Id.ToString();
        }

        public IFetchedJob FetchNextJob(string[] queues, CancellationToken cancellationToken)
        {
            return new ServiceFabricTransactionJob(queues, cancellationToken, this._jobQueueAppService);
        }

        public void SetJobParameter(string id, string name, string value)
        {
            var job = this._jobAppService.GetJobAsync(long.Parse(id)).GetAwaiter().GetResult();
            if (job == null)
                throw new ArgumentNullException(string.Format("JobId:{0},Not Found", id));
            if (job.Parameters == null)
                job.Parameters = new Dictionary<string, string>();
            job.Parameters.Add(name, value);
            this._jobAppService.UpdateJobAsync(job).GetAwaiter().GetResult();
        }

        public string GetJobParameter(string id, string name)
        {
            var job = this._jobAppService.GetJobAsync(long.Parse(id)).GetAwaiter().GetResult();
            if (job == null)
                throw new Exception("没有找到任务");
            var parameter = job.Parameters.FirstOrDefault(u => u.Key == name);
            return parameter.Value;

        }

        public JobData GetJobData(string jobId)
        {
            var job = this._jobAppService.GetJobAsync(long.Parse(jobId)).GetAwaiter().GetResult();
            var invocationData = InvocationData.DeserializePayload(job.InvocationData);
            var jobData = new JobData()
            {
                CreatedAt = job.CreatedAt,
                LoadException = null,
                Job = invocationData.DeserializeJob(),
                State = job.StateName
            };
            return jobData;
        }

        public StateData GetStateData(string jobId)
        {
            var stateData = this._jobStateDataAppService.GetLatestJobStateDataAsync(long.Parse(jobId)).GetAwaiter().GetResult();
            var data = new Dictionary<string, string>(SerializationHelper.Deserialize<Dictionary<string, string>>(stateData.Data), StringComparer.OrdinalIgnoreCase);

            return new StateData()
            {
                Name = stateData.Name,
                Reason = stateData.Reason,
                Data = data
            };
        }

        public void AnnounceServer(string serverId, ServerContext context)
        {
            var serverData = new ServerData()
            {
                Queues = context.Queues,
                WorkCount = context.WorkerCount,
                StartedAt = DateTime.UtcNow,
            };
            this._serverAppService.AddOrUpdateAsync(serverId, SerializationHelper.Serialize(serverData), DateTime.UtcNow).GetAwaiter().GetResult();

        }

        public void RemoveServer(string serverId)
        {
            this._serverAppService.RemoveServer(serverId);
        }

        public void Heartbeat(string serverId)
        {
            var server = this._serverAppService.GetServerAsync(serverId).GetAwaiter().GetResult();
            if (server == null)
                throw new Exception("Has not found that server,serverId:" + serverId);
            this._serverAppService.AddOrUpdateAsync(serverId, server.Data, DateTime.UtcNow);
        }

        public int RemoveTimedOutServers(TimeSpan timeOut)
        {
            if (timeOut.Duration() != timeOut)
            {
                throw new ArgumentException("The `timeOut` value must be positive.", nameof(timeOut));
            }
            int count = 0;
            var serverDtos = this._serverAppService.GetAllServerAsync().GetAwaiter().GetResult();
            foreach (var server in serverDtos)
            {
                if (server.LastHeartbeat < DateTime.UtcNow.Add(timeOut.Negate()))
                {
                    this._serverAppService.RemoveServer(server.ServerId).GetAwaiter().GetResult();
                    count++;
                }
            }
            return count;
        }

        public HashSet<string> GetAllItemsFromSet(string key)
        {
            var all_sets = this._jobSetsAppService.GetAllSetsAsync().GetAwaiter().GetResult();
            return all_sets.Where(u => u.Key == key).Select(u => u.Value).ToHashSet();
        }

        public string GetFirstByLowestScoreFromSet(string key, double fromScore, double toScore)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (toScore < fromScore) throw new ArgumentException("The `toScore` value must be higher or equal to the `fromScore` value.", nameof(toScore));

            var all_sets = this._jobSetsAppService.GetAllSetsAsync().GetAwaiter().GetResult();
            var firtst_set = all_sets.Where(u => u.Score >= fromScore && u.Score <= toScore).OrderBy(u => u.Score).FirstOrDefault();
            return firtst_set == null ? "" : firtst_set.Value;
        }

        public void SetRangeInHash(string key, IEnumerable<KeyValuePair<string, string>> keyValuePairs)
        {
            //normal,there must exited a blocked it
            this._hashAppService.AddOrUpdateAsync(key, keyValuePairs.ToDictionary(u => u.Key, u => u.Value)).GetAwaiter().GetResult();
        }

        public Dictionary<string, string> GetAllEntriesFromHash(string key)
        {
            var all_hash = this._hashAppService.GetAllHashAsync().GetAwaiter().GetResult();
            return all_hash.Where(u => u.Key == key).ToDictionary(u => u.Field, u => u.Value);
        }
    }
}
