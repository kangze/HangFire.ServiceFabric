using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HangFireStorageService.Dto;
using HangFireStorageService.Extensions;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;

namespace Hangfire.ServiceFabric.Servces
{
    public class AggregatedCounterAppService : IAggregatedCounterAppService
    {
        private readonly IReliableStateManager _stateManager;
        private readonly ServiceFabricOptions _option;

        public AggregatedCounterAppService(IReliableStateManager stateManager, ServiceFabricOptions option)
        {
            this._stateManager = stateManager;
            this._option = option;
        }

        public async Task<List<AggregatedCounterDto>> GetAllCounterAsync()
        {
            var aggregatedCounter_dict = await this._stateManager.GetOrAddAsync<IReliableDictionary2<long, AggregatedCounterDto>>(string.Format(Consts.AGGREGATEDCOUNTER, this._option.Prefix));
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
