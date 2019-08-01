using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hangfire.Annotations;
using Hangfire.Common;
using Hangfire.Server;
using Hangfire.ServiceFabric.Dtos;
using Hangfire.Storage;
using HangFireStorageService.Dto;
using HangFireStorageService.Servces;
using Mcs.Common.BaseServices;

namespace HangFireStorageService.Internal
{
    internal class ServiceFabricStorageConnect : JobStorageConnection
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

        public ServiceFabricWriteOnlyTransaction CreateTransaction()
        {
            return new ServiceFabricWriteOnlyTransaction(this._jobQueueAppService, this._jobAppService, this._jobStateDataAppService, this._serverAppService, this._counterAppService, this._aggregatedCounterAppService, this._jobSetsAppService, this._jobDataService, this._hashAppService, this._jobListAppService);
        }

        public override void Dispose()
        {

        }

        public override IWriteOnlyTransaction CreateWriteTransaction()
        {
            return new ServiceFabricWriteOnlyTransaction(this._jobQueueAppService, this._jobAppService, this._jobStateDataAppService, this._serverAppService, this._counterAppService, this._aggregatedCounterAppService, this._jobSetsAppService, this._jobDataService, this._hashAppService, this._jobListAppService);
        }

        public override IDisposable AcquireDistributedLock(string resource, TimeSpan timeout)
        {
            var locker = new ServicesFabricDistributedLock(null, resource, timeout);
            return locker.AcquireLock().GetAwaiter().GetResult();
        }

        public override string CreateExpiredJob(Job job, IDictionary<string, string> parameters, DateTime createdAt, TimeSpan expireIn)
        {
            using (var transaction = CreateTransaction())
            {
                var jobId = transaction.CreateExpiredJob(job, parameters, createdAt, expireIn);
                transaction.Commit();
                return jobId;
            }

        }

        public override IFetchedJob FetchNextJob(string[] queues, CancellationToken cancellationToken)
        {
            return new ServiceFabricTransactionJob(queues, cancellationToken, this._jobQueueAppService);
        }

        public override void SetJobParameter(string id, string name, string value)
        {
            using (var transaction = CreateTransaction())
            {
                transaction.SetJobParameter(id, name, value);
                transaction.Commit();
            }
        }

        public override string GetJobParameter(string id, string name)
        {
            var job = this._jobAppService.GetJobAsync(id).GetAwaiter().GetResult();
            if (job == null)
                throw new Exception("没有找到任务");
            var parameter = job.Parameters.FirstOrDefault(u => u.Key == name);
            return parameter.Value;

        }

        public override JobData GetJobData(string jobId)
        {
            if (string.IsNullOrEmpty(jobId))
                return null;
            var job = this._jobAppService.GetJobAsync(jobId).GetAwaiter().GetResult();
            if (job == null)
                return null;
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

        public override StateData GetStateData(string jobId)
        {
            if (jobId == null)
            {
                throw new ArgumentNullException(nameof(jobId));
            }

            var job = this._jobAppService.GetJobAsync(jobId).GetAwaiter().GetResult();

            if (job == null)
            {
                return null;
            }

            var state = job.StateHistory.LastOrDefault();

            if (state == null)
            {
                return null;
            }

            return new StateData
            {
                Name = state.Name,
                Reason = state.Reason,
                Data = state.Data
            };
        }

        public override void AnnounceServer(string serverId, ServerContext context)
        {
            var serverData = new ServerData()
            {
                Queues = context.Queues,
                WorkCount = context.WorkerCount,
                StartedAt = DateTime.UtcNow,
            };
            this._serverAppService.AddOrUpdateAsync(serverId, SerializationHelper.Serialize(serverData), DateTime.UtcNow).GetAwaiter().GetResult();

        }

        public override void RemoveServer(string serverId)
        {
            this._serverAppService.RemoveServer(serverId);
        }

        public override void Heartbeat(string serverId)
        {
            var server = this._serverAppService.GetServerAsync(serverId).GetAwaiter().GetResult();
            if (server == null)
                throw new Exception("Has not found that server,serverId:" + serverId);
            this._serverAppService.AddOrUpdateAsync(serverId, server.Data, DateTime.UtcNow);
        }

        public override int RemoveTimedOutServers(TimeSpan timeOut)
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

        public override HashSet<string> GetAllItemsFromSet(string key)
        {
            var all_sets = this._jobSetsAppService.GetAllSetsAsync().GetAwaiter().GetResult();
            return all_sets.Where(u => u.Key == key).Select(u => u.Value).ToHashSet();
        }

        public override string GetFirstByLowestScoreFromSet(string key, double fromScore, double toScore)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (toScore < fromScore) throw new ArgumentException("The `toScore` value must be higher or equal to the `fromScore` value.", nameof(toScore));

            var all_sets = this._jobSetsAppService.GetAllSetsAsync().GetAwaiter().GetResult();
            var firtst_set = all_sets.Where(u => u.Score >= fromScore && u.Score <= toScore).OrderBy(u => u.Score).FirstOrDefault();
            return firtst_set == null ? "" : firtst_set.Value;
        }

        public override void SetRangeInHash(string key, IEnumerable<KeyValuePair<string, string>> keyValuePairs)
        {
            //normal,there must exited a blocked it
            this._hashAppService.AddOrUpdateAsync(key, keyValuePairs.ToDictionary(u => u.Key, u => u.Value)).GetAwaiter().GetResult();
        }

        public override Dictionary<string, string> GetAllEntriesFromHash(string key)
        {
            var hash = this._hashAppService.GetHashDto(key).GetAwaiter().GetResult();
            return hash.Fields;
        }

        public override long GetCounter(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            var counter = this._counterAppService.GetCounterAsync(key).GetAwaiter().GetResult();

            return counter?.Value ?? 0;
        }

        public override long GetSetCount([NotNull] string key)
        {
            return 0;
        }

        public override List<string> GetRangeFromList([NotNull] string key, int startingFrom, int endingAt)
        {
            return new List<string>();
        }

        public override long GetHashCount([NotNull] string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            var hash = this._hashAppService.GetHashDto(key).GetAwaiter().GetResult();

            return hash?.Fields.Count ?? 0;
        }

        public override List<string> GetAllItemsFromList([NotNull] string key)
        {
            var list = this._jobListAppService.GetListDto(key).GetAwaiter().GetResult();
            return list.Select(u => u.Value).ToList();
        }

        public override List<string> GetFirstByLowestScoreFromSet(string key, double fromScore, double toScore, int count)
        {
            return new List<string>();
        }

        public override TimeSpan GetHashTtl([NotNull] string key)
        {
            return TimeSpan.FromSeconds(1);
        }

        public override long GetListCount([NotNull] string key)
        {
            return 0;
        }

        public override List<string> GetRangeFromSet([NotNull] string key, int startingFrom, int endingAt)
        {
            return base.GetRangeFromSet(key, startingFrom, endingAt);
        }

        public override TimeSpan GetListTtl([NotNull] string key)
        {
            return base.GetListTtl(key);
        }

        public override TimeSpan GetSetTtl([NotNull] string key)
        {
            return base.GetSetTtl(key);
        }

        public override string GetValueFromHash([NotNull] string key, [NotNull] string name)
        {
            return base.GetValueFromHash(key, name);
        }
    }
}
