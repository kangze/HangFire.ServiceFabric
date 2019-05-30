using HangFireStorageService.Dto;
using HangFireStorageService.Extensions;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HangFireStorageService.Servces
{
    public class JobQueueuAppService : IJobQueueAppService
    {
        private readonly IReliableStateManager _stateManager;
        private readonly ServiceFabricOptions _options;

        public JobQueueuAppService(IReliableStateManager stateManager, ServiceFabricOptions options)
        {
            this._stateManager = stateManager;
            this._options = options;
        }

        public async Task AddToQueueJObAsync(string queue, long jobId)
        {
            var queues_dict = await this._stateManager.GetOrAddAsync<IReliableDictionary2<string, List<JobQueueDto>>>(string.Format(Consts.JOBQUEUE_DICT, this._options.Prefix));
            using (var tx = this._stateManager.CreateTransaction())
            {
                var queues_condition = await queues_dict.TryGetValueAsync(tx, queue);
                if (!queues_condition.HasValue)
                    return;
                queues_condition.Value.Add(new JobQueueDto()
                {
                    Queue = queue,
                    JobId = jobId,
                });
                await queues_dict.SetAsync(tx, queue, queues_condition.Value);
                await tx.CommitAsync();
            }
        }

        public async Task DeleteQueueJobAsync(string queue, long jobId)
        {
            var queues_dict = await this._stateManager.GetOrAddAsync<IReliableDictionary2<string, List<JobQueueDto>>>(string.Format(Consts.JOBQUEUE_DICT, this._options.Prefix));
            var queue_jobs = await this.GetQueuesAsync(queue);
            queue_jobs.RemoveAll(u => u.JobId == jobId);
            using (var tx = this._stateManager.CreateTransaction())
            {
                await queues_dict.SetAsync(tx, queue, queue_jobs);
                await tx.CommitAsync();
            }
        }

        public async Task<List<JobQueueDto>> GetQueuesAsync(string queue)
        {
            var queues_dict = await this._stateManager.GetOrAddAsync<IReliableDictionary2<string, List<JobQueueDto>>>(string.Format(Consts.JOBQUEUE_DICT, this._options.Prefix));
            using (var tx = this._stateManager.CreateTransaction())
            {
                var enumerator = (await queues_dict.CreateEnumerableAsync(tx)).GetAsyncEnumerator();
                var ls = new List<JobQueueDto>();
                while (await enumerator.MoveNextAsync(default))
                {
                    if (string.IsNullOrEmpty(queue))
                        ls.AddRange(enumerator.Current.Value);
                    else
                        ls.AddRange(enumerator.Current.Value.Where(u => u.Queue == queue).ToList());
                }
                return ls;
            }
        }
    }
}
