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
    public class AggregatedCounterAppService : IAggregatedCounterAppService
    {
        private readonly IReliableStateManager _stateManager;
        private readonly string _dictName;

        public AggregatedCounterAppService(IReliableStateManager stateManager, string dictName)
        {
            this._stateManager = stateManager;
            _dictName = dictName;
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
