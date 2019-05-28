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

        public async Task<List<JobQueueDto>> GetQueuesAsync(string queue)
        {
            var queues_dict = await this._stateManager.GetOrAddAsync<IReliableDictionary2<long, List<JobQueueDto>>>(string.Format(Consts.JOBQUEUE_DICT, this._options.Prefix));
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
