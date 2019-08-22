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
    public class JobSetAppService : IJobSetAppService
    {
        private readonly IReliableStateManager _stateManager;
        private readonly string _dictName;

        public JobSetAppService(IReliableStateManager stateManager, string dictName)
        {
            this._stateManager = stateManager;
            this._dictName = dictName;

        }

        public async Task AddSetAsync(ITransaction tx, IReliableDictionary2<string, SetDto> setDtoDict, SetDto setDto)
        {
            var key = setDto.Key + setDto.Value;
            var setCondition = await setDtoDict.TryGetValueAsync(tx, key);
            if (setCondition.HasValue)
            {
                setCondition.Value.Score = setDto.Score;
                await setDtoDict.SetAsync(tx, key, setCondition.Value);
            }
            else
            {
                await setDtoDict.SetAsync(tx, key, setDto);
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

        public async Task RemoveAsync(ITransaction tx, IReliableDictionary2<string, SetDto> setDtoDict, SetDto setDto)
        {
            await setDtoDict.TryRemoveAsync(tx, setDto.Key + setDto.Value);
        }

        private async Task<IReliableDictionary2<string, SetDto>> GetSetDtosDictAsync()
        {
            var set_dict = await this._stateManager.GetOrAddAsync<IReliableDictionary2<string, SetDto>>(Consts.SET_DICT);
            return set_dict;
        }
    }
}
