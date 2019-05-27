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
    public class AggregatedCounterAppService : IAggregatedCounterAppService
    {
        private readonly IReliableStateManager _stateManager;

        public AggregatedCounterAppService(IReliableStateManager stateManager)
        {
            this._stateManager = stateManager;
        }

        public async Task<List<AggregatedCounterDto>> GetAllCounterAsync()
        {
            var aggregatedCounter_dict = await this._stateManager.GetOrAddAsync<IReliableDictionary2<long, AggregatedCounterDto>>(Consts.AGGREGATEDCOUNTER);
            using (var tx = this._stateManager.CreateTransaction())
            {
                var ls = new List<AggregatedCounterDto>();
                var enumlator = (await aggregatedCounter_dict.CreateEnumerableAsync(tx)).GetAsyncEnumerator();
                while (await enumlator.MoveNextAsync(default))
                {
                    ls.Add(enumlator.Current.Value);
                }
                return ls;
            }

        }
    }
}
