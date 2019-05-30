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
    public class HashAppService : IHashAppService
    {
        private readonly IReliableStateManager _stateManager;

        public HashAppService(IReliableStateManager stateManager)
        {
            _stateManager = stateManager;
        }

        public async Task RemoveAsync(string key)
        {
            var hash_dict = await this._stateManager.GetOrAddAsync<IReliableDictionary2<string, List<HashDto>>>(Consts.HASH_DICT);
            using (var tx = this._stateManager.CreateTransaction())
            {
                var hash_condition = await hash_dict.TryGetValueAsync(tx, key);
                if (!hash_condition.HasValue)
                    return;
                await hash_dict.TryRemoveAsync(tx, key);
                await tx.CommitAsync();
                return;
            }
        }

        public async Task<List<HashDto>> GetAllHashAsync()
        {
            var hash_dict = await this._stateManager.GetOrAddAsync<IReliableDictionary2<string, List<HashDto>>>(Consts.HASH_DICT);
            var ls = new List<HashDto>();
            using (var tx = this._stateManager.CreateTransaction())
            {
                var emulator = (await hash_dict.CreateEnumerableAsync(tx)).GetAsyncEnumerator();
                while (await emulator.MoveNextAsync(default))
                {
                    ls.AddRange(emulator.Current.Value);
                }
            }
            return ls;
        }

        public async Task AddOrUpdateAsync(string key, Dictionary<string, string> dict)
        {

            var hash_dict = await this._stateManager.GetOrAddAsync<IReliableDictionary2<string, List<HashDto>>>(Consts.HASH_DICT);
            using (var tx = this._stateManager.CreateTransaction())
            {
                var emulator = (await hash_dict.CreateEnumerableAsync(tx)).GetAsyncEnumerator();
                while (await emulator.MoveNextAsync(default))
                {
                    var current = emulator.Current.Value;
                    if (emulator.Current.Key == key)
                    {
                        foreach (var item in dict)
                        {
                            var key_value_pair = emulator.Current.Value.FirstOrDefault(u => u.Key == item.Key);
                            if (key_value_pair != null) key_value_pair.Value = item.Value;
                            else current.Add(new HashDto() { Key = key, Field = item.Key, Value = item.Value });
                        }
                        await hash_dict.SetAsync(tx, key, current);
                        await tx.CommitAsync();
                        return;
                    }
                }

                //There must has add it
                var hashDtos = dict.Select(u => new HashDto()
                {
                    Key = key,
                    Field = u.Key,
                    Value = u.Value
                }).ToList();
                await hash_dict.SetAsync(tx, key, hashDtos);
                await tx.CommitAsync();
            }
        }
    }
}
