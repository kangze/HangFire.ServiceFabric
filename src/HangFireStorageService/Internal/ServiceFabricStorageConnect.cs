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
        private readonly IJobStateDataAppService _jobStateDataAppService;

        public ServiceFabricStorageConnect(
            IJobDataService jobDataService,
            IJobAppService jobAppService,
            IJobStateDataAppService jobStateDataAppService
            )
        {
            this._jobDataService = jobDataService;
            this._jobAppService = jobAppService;
            this._jobStateDataAppService = jobStateDataAppService;
        }

        public void Dispose()
        {

        }

        public IWriteOnlyTransaction CreateWriteTransaction()
        {
            return new ServiceFabricWriteOnlyTransaction(new List<OperationDto>(), new JobDataService(null));
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
            var parametersArrary = parameters.ToArray();
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
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public void RemoveServer(string serverId)
        {
            throw new NotImplementedException();
        }

        public void Heartbeat(string serverId)
        {
            throw new NotImplementedException();
        }

        public int RemoveTimedOutServers(TimeSpan timeOut)
        {
            throw new NotImplementedException();
        }

        public HashSet<string> GetAllItemsFromSet(string key)
        {
            throw new NotImplementedException();
        }

        public string GetFirstByLowestScoreFromSet(string key, double fromScore, double toScore)
        {
            throw new NotImplementedException();
        }

        public void SetRangeInHash(string key, IEnumerable<KeyValuePair<string, string>> keyValuePairs)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, string> GetAllEntriesFromHash(string key)
        {
            throw new NotImplementedException();
        }
    }
}
