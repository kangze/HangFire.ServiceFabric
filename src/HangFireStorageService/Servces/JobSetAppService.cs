using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire.ServiceFabric.Dtos;
using HangFireStorageService.Dto;
using HangFireStorageService.Extensions;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;

namespace Hangfire.ServiceFabric.Servces
{
    public class JobSetAppService : IJobSetAppService
    {
        private readonly IReliableStateManager _stateManager;
        private readonly ServiceFabricOptions _option;

        public JobSetAppService(IReliableStateManager stateManager, ServiceFabricOptions option)
        {
            this._stateManager = stateManager;
            this._option = option;
        }

        public async Task AddSetAsync(string key, string value, double score)
        {
            var set_dict = await GetSetDtosDictAsync();
            using (var tx = this._stateManager.CreateTransaction())
            {
                var set_condition = await set_dict.TryGetValueAsync(tx, key);
                if (set_condition.HasValue)
                {
                    set_condition.Value.Score = score;
                    await set_dict.SetAsync(tx, key, set_condition.Value);
                    await tx.CommitAsync();
                }
                else
                {
                    await set_dict.SetAsync(tx, key, new SetDto()
                    {
                        Key = key,
                        Value = value,
                        Score = score
                    });
                    await tx.CommitAsync();
                }
            }

        }

        public async Task<List<SetDto>> GetSetsAsync()
        {
            var set_dict = await GetSetDtosDictAsync();
            var ls = new List<SetDto>();
            using (var tx = this._stateManager.CreateTransaction())
            {
                var emulator = (await set_dict.CreateEnumerableAsync(tx)).GetAsyncEnumerator();
                while (await emulator.MoveNextAsync(default))
                {
                    ls.Add(emulator.Current.Value);
                }
            }
            return ls;
        }

        public async Task RemoveAsync(string key, string value)
        {
            var set_dict = await GetSetDtosDictAsync();
            var ls = new List<SetDto>();
            using (var tx = this._stateManager.CreateTransaction())
            {
                await set_dict.TryRemoveAsync(tx, key);
                await tx.CommitAsync();
            }
        }

        private async Task<IReliableDictionary2<string, SetDto>> GetSetDtosDictAsync()
        {
            var set_dict = await this._stateManager.GetOrAddAsync<IReliableDictionary2<string, SetDto>>(string.Format(Consts.SET_DICT, this._option.Prefix));
            return set_dict;
        }
    }
}
