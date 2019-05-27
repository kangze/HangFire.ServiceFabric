using HangFireStorageService.Dto;
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

        public JobQueueuAppService(IReliableStateManager stateManager)
        {
            this._stateManager = stateManager;
        }

        public async Task<List<JobQuequeDto>> GetAllJobQueueusAsync()
        {
            //TODO:还必须加入一个去重的操作
            var queues_dict = await this._stateManager.GetOrAddAsync<IReliableDictionary2<long, List<JobQuequeDto>>>(Consts.JOBQUEUE_DICT);
            using (var tx = this._stateManager.CreateTransaction())
            {
                var emulator = (await queues_dict.CreateEnumerableAsync(tx)).GetAsyncEnumerator();
                var ls = new List<JobQuequeDto>();
                while (await emulator.MoveNextAsync(default))
                {
                    ls.AddRange(emulator.Current.Value);
                }
                return ls;
            }
        }
    }
}
