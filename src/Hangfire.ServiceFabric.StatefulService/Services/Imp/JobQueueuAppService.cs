using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire.ServiceFabric.Extensions;
using Hangfire.ServiceFabric.Model;
using Hangfire.ServiceFabric.Model.Dtos;
using Hangfire.ServiceFabric.Model.Interfaces;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;

namespace Hangfire.ServiceFabric.StatefulService.Services.Imp
{
    public class JobQueueAppService : IJobQueueAppService
    {
        private readonly IReliableStateManager _stateManager;
        private readonly string _dictName;

        public JobQueueAppService(IReliableStateManager stateManager, string dictName)
        {
            this._stateManager = stateManager;
            _dictName = dictName;
        }

        public async Task AddToQueueJObAsync(string queue, string jobId)
        {
            var queuesDict = await StoreFactory.CreateJobQueueDictAsync(this._stateManager, this._dictName);
            using (var tx = this._stateManager.CreateTransaction())
            {
                var dto = new JobQueueDto()
                {
                    Id = Guid.NewGuid().ToString("N"),
                    Queue = queue,
                    JobId = jobId
                };
                await queuesDict.SetAsync(tx, dto.Id, dto);
                await tx.CommitAsync();
            }
        }

        public async Task<List<JobQueueDto>> GetQueuesAsync(string queue)
        {
            var queuesDict = await StoreFactory.CreateJobQueueDictAsync(this._stateManager, this._dictName);
            using (var tx = this._stateManager.CreateTransaction())
            {
                var enumerator = (await queuesDict.CreateEnumerableAsync(tx)).GetAsyncEnumerator();
                var ls = new List<JobQueueDto>();
                while (await enumerator.MoveNextAsync(default))
                {
                    if (string.IsNullOrEmpty(queue) || enumerator.Current.Value.Queue == queue)
                        ls.Add(enumerator.Current.Value);
                }
                return ls;
            }
        }

        public async Task<List<JobQueueDto>> GetEnqueuedJob(string queue, int from, int perPage)
        {
            var queues = await this.GetQueuesAsync(null);
            var result = queues.Where(u => u.FetchedAt == null && u.Queue == queue)
                .Skip(from)
                .Take(perPage)
                .ToList();
            return result;
        }

        public async Task<EnqueuedAndFetchedCountDto> GetEnqueuedAndFetchedCount(string queue)
        {
            var queues = await this.GetQueuesAsync(queue);
            return new EnqueuedAndFetchedCountDto
            {
                EnqueuedCount = queues.Count(u => u.FetchedAt == null),
                FetchedCount = queues.Count(u => u.FetchedAt != null)
            };
        }

        public async Task<List<string>> GetFetchedJobIds(string queue, int from, int perPage)
        {
            var queues = await this.GetQueuesAsync(null);
            var result = queues.Where(u => u.FetchedAt != null && u.Queue == queue)
                .Skip(from)
                .Take(perPage)
                .ToList();
            //TODO:check jobId whether exist
            return result.Select(u => u.JobId).ToList();
        }

        public async Task<JobQueueDto> GetQueueAsync(string id)
        {
            var queuesDict = await StoreFactory.CreateJobQueueDictAsync(this._stateManager, this._dictName);
            using (var tx = this._stateManager.CreateTransaction())
            {
                var queueCondition = await queuesDict.TryGetValueAsync(tx, id);
                if (queueCondition.HasValue)
                    return queueCondition.Value;
                return null;
            }
        }

        public async Task DeleteQueueJobAsync(string id)
        {
            var queuesDict = await StoreFactory.CreateJobQueueDictAsync(this._stateManager, this._dictName);
            using (var tx = this._stateManager.CreateTransaction())
            {
                await queuesDict.TryRemoveAsync(tx, id);
                await tx.CommitAsync();
            }
        }

        public async Task UpdateQueueAsync(JobQueueDto dto)
        {
            var queuesDict = await StoreFactory.CreateJobQueueDictAsync(this._stateManager, this._dictName);
            using (var tx = this._stateManager.CreateTransaction())
            {
                await queuesDict.SetAsync(tx, dto.Id, dto);
                await tx.CommitAsync();
            }

        }

        public async Task<JobQueueDto> GetFetchedJobAsync(string queue)
        {
            var queuesDict = await StoreFactory.CreateJobQueueDictAsync(this._stateManager, this._dictName);
            using (var tx = this._stateManager.CreateTransaction())
            {
                var enumerator = (await queuesDict.CreateEnumerableAsync(tx)).GetAsyncEnumerator();
                while (await enumerator.MoveNextAsync(default))
                {
                    if (!string.IsNullOrEmpty(queue) && enumerator.Current.Value.Queue == queue && enumerator.Current.Value.FetchedAt == null)
                        return enumerator.Current.Value;
                }
                return null;
            }
        }

        public async Task AddAsync(ITransaction tx, IReliableDictionary2<string, JobQueueDto> jobQueueDict, string id, string jobId, string queue)
        {
            var dto = new JobQueueDto()
            {
                Id = id,
                Queue = queue,
                JobId = jobId
            };
            await jobQueueDict.SetAsync(tx, dto.Id, dto);
        }
    }
}
