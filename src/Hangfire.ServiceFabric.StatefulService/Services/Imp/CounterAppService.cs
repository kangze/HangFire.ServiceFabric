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

        public async Task AddAsync(string key, TimeSpan? expireIn)
        {
            var counter_dict = await GetCounterDtosDictAsync();
            using (var tx = this._stateManager.CreateTransaction())
            {
                var dto = new CounterDto();
                dto.Id = Guid.NewGuid().ToString("N");
                dto.Key = key;
                dto.Value = 1;
                dto.ExpireAt = expireIn.HasValue ? (DateTime?)DateTime.UtcNow.Add(expireIn.Value) : null;
                await counter_dict.SetAsync(tx, dto.Id, dto);
                await tx.CommitAsync();
            }
        }

        public async Task DecrementAsync(string key, long amount, TimeSpan? expireIn)
        {
            var counter_dict = await GetCounterDtosDictAsync();
            using (var tx = this._stateManager.CreateTransaction())
            {
                var enumlater = (await counter_dict.CreateEnumerableAsync(tx)).GetAsyncEnumerator();
                while (await enumlater.MoveNextAsync(default))
                {
                    if (enumlater.Current.Value.Key == key)
                    {
                        var existedDto = enumlater.Current.Value;
                        existedDto.Value += amount;
                        if (expireIn != null)
                        {
                            existedDto.ExpireAt = DateTime.UtcNow.Add(expireIn.Value);
                        }
                        await counter_dict.SetAsync(tx, existedDto.Id, existedDto);
                        await tx.CommitAsync();
                        return;
                    }

                }

                //to add
                var dto = new CounterDto()
                {
                    Id = Guid.NewGuid().ToString("N"),
                    Key = key,
                    Value = amount
                };
                if (expireIn != null)
                {
                    dto.ExpireAt = DateTime.UtcNow.Add(expireIn.Value);
                }
                await counter_dict.SetAsync(tx, dto.Id, dto);
                await tx.CommitAsync();
            }

            using (var tx = this._stateManager.CreateTransaction())
            {
                var dto = new CounterDto();
                dto.Id = Guid.NewGuid().ToString("N");
                dto.Key = key;
                dto.Value = dto.Value - 1;
                dto.ExpireAt = expireIn.HasValue ? (DateTime?)DateTime.UtcNow.Add(expireIn.Value) : null;
                await counter_dict.SetAsync(tx, dto.Id, dto);
                await tx.CommitAsync();
            }
        }

        public async Task<List<CounterDto>> GetAllCounterAsync()
        {
            var counter_dict = await GetCounterDtosDictAsync();
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

        public async Task<CounterDto> GetCounterAsync(string key)
        {
            var counter_dict = await GetCounterDtosDictAsync();
            using (var tx = this._stateManager.CreateTransaction())
            {
                var ls = new List<CounterDto>();
                var enumlater = (await counter_dict.CreateEnumerableAsync(tx)).GetAsyncEnumerator();
                while (await enumlater.MoveNextAsync(default))
                {
                    if (enumlater.Current.Value.Key == key)
                        return enumlater.Current.Value;
                }
                return null;
            }
        }

        private async Task<IReliableDictionary2<string, CounterDto>> GetCounterDtosDictAsync()
        {
            var counter_dict = await this._stateManager.GetOrAddAsync<IReliableDictionary2<string, CounterDto>>(Consts.COUNTER);
            return counter_dict;
        }
    }
}
