﻿using System;
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
    public class HashAppService : IHashAppService
    {
        private readonly IReliableStateManager _stateManager;

        public HashAppService(IReliableStateManager stateManager)
        {
            this._stateManager = stateManager;
        }

        public async Task RemoveAsync(string key)
        {
            var hash_dict = await GetHashDtosDictAsync();
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
            var hash_dict = await GetHashDtosDictAsync();
            var ls = new List<HashDto>();
            using (var tx = this._stateManager.CreateTransaction())
            {
                var emulator = (await hash_dict.CreateEnumerableAsync(tx)).GetAsyncEnumerator();
                while (await emulator.MoveNextAsync(default))
                {
                    ls.Add(emulator.Current.Value);
                }
            }
            return ls;
        }

        public async Task AddOrUpdateAsync(HashDto dto)
        {
            var hash_dict = await GetHashDtosDictAsync();
            using (var tx = this._stateManager.CreateTransaction())
            {
                await hash_dict.SetAsync(tx, dto.Id, dto);
                await tx.CommitAsync();
            }
        }

        public async Task<HashDto> GetHashDtoAsync(string key)
        {
            var hash_dict = await GetHashDtosDictAsync();
            using (var tx = this._stateManager.CreateTransaction())
            {
                var emulator = (await hash_dict.CreateEnumerableAsync(tx)).GetAsyncEnumerator();
                while (await emulator.MoveNextAsync(default))
                {
                    if (emulator.Current.Value.Key == key)
                        return emulator.Current.Value;
                }
                return null;
            }
        }

        private async Task<IReliableDictionary2<string, HashDto>> GetHashDtosDictAsync()
        {
            var hash_dict = await this._stateManager.GetOrAddAsync<IReliableDictionary2<string, HashDto>>(Consts.HASH_DICT);
            return hash_dict;
        }
    }
}