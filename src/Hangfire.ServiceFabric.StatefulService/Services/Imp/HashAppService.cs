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
    public class HashAppService : IHashAppService
    {
        private readonly IReliableStateManager _stateManager;
        private readonly string _dictName;

        public HashAppService(IReliableStateManager stateManager, string dictName)
        {
            this._stateManager = stateManager;
            _dictName = dictName;
        }

        public async Task<HashDto> GetHashDtoAsync(string key)
        {
            var hashDict = await StoreFactory.CreateHashDictAsync(this._stateManager, this._dictName);
            using (var tx = this._stateManager.CreateTransaction())
            {
                var emulator = (await hashDict.CreateEnumerableAsync(tx)).GetAsyncEnumerator();
                while (await emulator.MoveNextAsync(default))
                {
                    if (emulator.Current.Value.Key == key)
                        return emulator.Current.Value;
                }
                return null;
            }
        }

        public async Task<List<HashDto>> GetAllHashAsync()
        {
            var hashDict = await StoreFactory.CreateHashDictAsync(this._stateManager, this._dictName);
            var ls = new List<HashDto>();
            using (var tx = this._stateManager.CreateTransaction())
            {
                var emulator = (await hashDict.CreateEnumerableAsync(tx)).GetAsyncEnumerator();
                while (await emulator.MoveNextAsync(default))
                {
                    ls.Add(emulator.Current.Value);
                }
            }
            return ls;
        }

        public async Task RemoveAsync(ITransaction tx, IReliableDictionary2<string, HashDto> dict, string key)
        {
            await dict.TryRemoveAsync(tx, key);
        }

        public async Task SetRangInHash(ITransaction tx, IReliableDictionary2<string, HashDto> dict, string key, IEnumerable<KeyValuePair<string, string>> keyValuePairs)
        {
            var hashDto = await dict.TryGetValueAsync(tx, key);
            if (!hashDto.HasValue)
                return;
            foreach (var pair in keyValuePairs)
                hashDto.Value.Fields.AddOrUpdate(pair.Key, pair.Value);
            await dict.AddOrUpdateAsync(tx, key, hashDto.Value, (k, v) => hashDto.Value);
        }
    }
}