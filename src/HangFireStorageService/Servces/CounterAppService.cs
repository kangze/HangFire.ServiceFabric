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

        public async Task AddAsync(string key, TimeSpan? expireIn)
        {
            var counter_dict = await this._stateManager.GetOrAddAsync<IReliableDictionary2<long, CounterDto>>(Consts.COUNTER);
            using (var tx = this._stateManager.CreateTransaction())
            {
                var dto = new CounterDto();
                var count = await counter_dict.GetCountAsync(tx);
                dto.Id = count + 1;
                dto.Key = key;
                dto.Value = 1;
                dto.ExpireAt = expireIn.HasValue ? (DateTime?)DateTime.UtcNow.Add(expireIn.Value) : null;
                await counter_dict.SetAsync(tx, count + 1, dto);
                await tx.CommitAsync();
            }
        }

        public async Task DeleteAsync(string key, TimeSpan? expireIn)
        {
            var counter_dict = await this._stateManager.GetOrAddAsync<IReliableDictionary2<long, CounterDto>>(Consts.COUNTER);
            using (var tx = this._stateManager.CreateTransaction())
            {
                var dto = new CounterDto();
                var count = await counter_dict.GetCountAsync(tx);
                dto.Id = count + 1;
                dto.Key = key;
                dto.Value = -1;
                dto.ExpireAt = expireIn.HasValue ? (DateTime?)DateTime.UtcNow.Add(expireIn.Value) : null;
                await counter_dict.SetAsync(tx, count + 1, dto);
                await tx.CommitAsync();
            }
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
