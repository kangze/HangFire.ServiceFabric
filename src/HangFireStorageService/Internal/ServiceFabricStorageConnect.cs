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

        public ServiceFabricStorageConnect(IJobDataService jobDataService)
        {
            this._jobDataService = jobDataService;
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
            this._jobDataService.AddOrUpdateJobParameter(id, name, value).GetAwaiter().GetResult();

        }

        public string GetJobParameter(string id, string name)
        {
            var value = this._jobDataService.GetJobParameter(id, name).GetAwaiter().GetResult();
            return value;
        }

        public JobData GetJobData(string jobId)
        {
            var job = this._jobDataService.GetJobAsync(long.Parse(jobId)).GetAwaiter().GetResult();
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
            throw new NotImplementedException();
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
