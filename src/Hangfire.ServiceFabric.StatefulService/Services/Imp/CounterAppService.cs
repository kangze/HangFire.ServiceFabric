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
    public class CounterAppService : ICounterAppService
    {
        private readonly IReliableStateManager _stateManager;
        private readonly string _dictName;

        public CounterAppService(IReliableStateManager stateManager, string dictName)
        {
            this._stateManager = stateManager;
            this._dictName = dictName;
        }

        public async Task<CounterDto> GetCounterAsync(string key)
        {
            var counterDict = await StoreFactory.CreateCounterDictAsync(this._stateManager, this._dictName);
            using (var tx = this._stateManager.CreateTransaction())
            {
                var ls = new List<CounterDto>();
                var enumerater = (await counterDict.CreateEnumerableAsync(tx)).GetAsyncEnumerator();
                while (await enumerater.MoveNextAsync(default))
                {
                    if (enumerater.Current.Value.Key == key)
                        return enumerater.Current.Value;
                }
                return null;
            }
        }

        public async Task<List<CounterDto>> GetAllCounterAsync()
        {
            var counterDict = await StoreFactory.CreateCounterDictAsync(this._stateManager, this._dictName);
            using (var tx = this._stateManager.CreateTransaction())
            {
                var ls = new List<CounterDto>();
                var enumlater = (await counterDict.CreateEnumerableAsync(tx)).GetAsyncEnumerator();
                while (await enumlater.MoveNextAsync(default))
                {
                    ls.Add(enumlater.Current.Value);
                }
                return ls;
            }
        }

        public async Task AddAsync(ITransaction tx, IReliableDictionary2<string, CounterDto> counterDict, CounterDto dto)
        {
            await counterDict.AddOrUpdateAsync(tx, dto.Id, dto, (k, v) => dto);
        }
    }
}
