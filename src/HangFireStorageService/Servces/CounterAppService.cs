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
    public class CounterAppService : ICounterAppService
    {
        private readonly IReliableStateManager _stateManager;

        public CounterAppService(IReliableStateManager stateManager)
        {
            this._stateManager = stateManager;
        }

        public async Task<List<CounterDto>> GetAllCounterAsync()
        {
            var counter_dict = await this._stateManager.GetOrAddAsync<IReliableDictionary2<long, CounterDto>>(Consts.COUNTER);
            using (var tx = this._stateManager.CreateTransaction())
            {
                var ls = new List<CounterDto>();
                var enumlater = (await counter_dict.CreateEnumerableAsync(tx)).GetAsyncEnumerator();
                while (await enumlater.MoveNextAsync(default))
                {
                    ls.Add(enumlater.Current.Value);
                }
                return ls;
            }
        }
    }
}
