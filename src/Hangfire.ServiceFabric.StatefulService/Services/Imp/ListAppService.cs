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
    public class ListAppService : IListAppService
    {
        private readonly IReliableStateManager _stateManager;
        private readonly string _dictName;

        public ListAppService(IReliableStateManager stateManager, string dictName)
        {
            _stateManager = stateManager;
            _dictName = dictName;
        }

        public async Task AddAsync(ITransaction tx, IReliableDictionary2<string, ListDto> listDict, ListDto dto)
        {
            await listDict.AddOrUpdateAsync(tx, dto.Id, dto, (k, v) => dto);
        }

        public async Task<List<ListDto>> GetListDtoAsync(string key)
        {
            var list_dict = await this._stateManager.GetOrAddAsync<IReliableDictionary2<string, ListDto>>(Consts.LIST_DICT);
            using (var tx = this._stateManager.CreateTransaction())
            {
                var emulator = (await list_dict.CreateEnumerableAsync(tx)).GetAsyncEnumerator();
                var list = new List<ListDto>();
                while (await emulator.MoveNextAsync(default))
                {
                    if (emulator.Current.Value.Item == key)
                        list.Add(emulator.Current.Value);
                }
                return list;
            }
        }

        public async Task Remove(ITransaction tx, IReliableDictionary2<string, ListDto> listDict, ListDto dto)
        {
            var emulator = (await listDict.CreateEnumerableAsync(tx)).GetAsyncEnumerator();
            string id = string.Empty;
            while (await emulator.MoveNextAsync(default))
            {
                if (emulator.Current.Value.Item == dto.Item && emulator.Current.Value.Value == dto.Value)
                {
                    id = emulator.Current.Value.Id;
                    break;
                }

            }

            if (!string.IsNullOrEmpty(id))
                await listDict.TryRemoveAsync(tx, id);
        }

        public async Task Remove(string key, string value)
        {
            var list_dict = await this._stateManager.GetOrAddAsync<IReliableDictionary2<string, JobListDto>>(Consts.LIST_DICT);
            using (var tx = this._stateManager.CreateTransaction())
            {
                await list_dict.TryRemoveAsync(tx, key);
                await tx.CommitAsync();
            }
        }

        public async Task RemoveRange(ITransaction tx, IReliableDictionary2<string, ListDto> listDict, string key, int keepStartingFrom, int keepEndingAt)
        {

            var emulator = (await listDict.CreateEnumerableAsync(tx)).GetAsyncEnumerator();
            var ls = new List<ListDto>();
            while (await emulator.MoveNextAsync(default))
                ls.Add(emulator.Current.Value);
            var removed = ls.OrderBy(u => u.Item).Skip(keepEndingAt).Take(keepEndingAt - keepStartingFrom).ToList();
            foreach (var re in removed)
                await listDict.TryRemoveAsync(tx, re.Item);

        }
    }
}
