using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HangFireStorageService.Dto;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;

namespace HangFireStorageService.Servces
{
    public class JobSetAppService : IJobSetAppService
    {
        private readonly IReliableStateManager _stateManager;

        public JobSetAppService(IReliableStateManager stateManager)
        {
            this._stateManager = stateManager;
        }

        public async Task<List<SetDto>> GetAllSetsAsync()
        {
            var set_dict = await this._stateManager.GetOrAddAsync<IReliableDictionary2<long, SetDto>>(Consts.SET_DICT);
            var ls = new List<SetDto>();
            using (var tx = this._stateManager.CreateTransaction())
            {
                var emulator = (await set_dict.CreateEnumerableAsync(tx)).GetAsyncEnumerator();
                while (await emulator.MoveNextAsync(default))
                {
                    ls.Add(emulator.Current.Value);
                }
            }
            return ls;
        }

        
    }
}
