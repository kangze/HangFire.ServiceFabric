using Hangfire.ServiceFabric.Dtos;
using Hangfire.ServiceFabric.Entities;
using HangFireStorageService.Dto;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.ServiceFabric.Services
{
    public class JobQueueuAppService : IJobQueueAppService
    {
        private readonly IReliableStateManager _stateManager;

        public JobQueueuAppService(IReliableStateManager stateManager)
        {
            this._stateManager = stateManager;
        }

        public async Task AddToQueueJObAsync(string queue, string jobId)
        {
            var queues_dict = await this._stateManager.GetOrAddAsync<IReliableDictionary2<string, JobQueueDto>>(Consts.JOBQUEUE_DICT);
            using (var tx = this._stateManager.CreateTransaction())
            {
                var dto = new JobQueueDto()
                {
                    Id = Guid.NewGuid().ToString("N"),
                    Queue = queue,
                    JobId = jobId
                };
                await queues_dict.SetAsync(tx, dto.Id, dto);
                await tx.CommitAsync();
            }
        }

        public async Task<List<JobQueueDto>> GetQueuesAsync(string queue)
        {
            var queues_dict = await this._stateManager.GetOrAddAsync<IReliableDictionary2<string, JobQueueDto>>(Consts.JOBQUEUE_DICT);
            using (var tx = this._stateManager.CreateTransaction())
            {
                var enumerator = (await queues_dict.CreateEnumerableAsync(tx)).GetAsyncEnumerator();
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
            var queues_dict = await this._stateManager.GetOrAddAsync<IReliableDictionary2<string, JobQueueDto>>(Consts.JOBQUEUE_DICT);
            using (var tx = this._stateManager.CreateTransaction())
            {
                var condition_value = await queues_dict.TryGetValueAsync(tx, id);
                if (condition_value.HasValue)
                    return condition_value.Value;
                return null;
            }
        }

        public async Task DeleteQueueJobAsync(string id)
        {
            var queues_dict = await this._stateManager.GetOrAddAsync<IReliableDictionary2<string, JobQueueDto>>(Consts.JOBQUEUE_DICT);
            using (var tx = this._stateManager.CreateTransaction())
            {
                await queues_dict.TryRemoveAsync(tx, id);
                await tx.CommitAsync();
            }
        }

        public async Task UpdateQueueAsync(JobQueueDto dto)
        {
            var queues_dict = await this._stateManager.GetOrAddAsync<IReliableDictionary2<string, JobQueueDto>>(Consts.JOBQUEUE_DICT);
            using (var tx = this._stateManager.CreateTransaction())
            {
                await queues_dict.SetAsync(tx, dto.Id, dto);
                await tx.CommitAsync();
            }

        }

        public async Task<JobQueueDto> GetFetchedJobAsync(string queue)
        {
            var queues_dict = await this._stateManager.GetOrAddAsync<IReliableDictionary2<string, JobQueueDto>>(Consts.JOBQUEUE_DICT);
            using (var tx = this._stateManager.CreateTransaction())
            {
                var enumerator = (await queues_dict.CreateEnumerableAsync(tx)).GetAsyncEnumerator();
                while (await enumerator.MoveNextAsync(default))
                {
                    if (!string.IsNullOrEmpty(queue) && enumerator.Current.Value.Queue == queue && enumerator.Current.Value.FetchedAt == null)
                        return enumerator.Current.Value;
                }
                return null;
            }
        }
    }
}
