using HangFireStorageService.Dto;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HangFireStorageService.Servces
{
    public class JobListAppService : IJobListAppService
    {
        private readonly IReliableStateManager _stateManager;

        public JobListAppService(IReliableStateManager stateManager)
        {
            _stateManager = stateManager;
        }

        public async Task AddAsync(string key, string value)
        {
            var list_dict = await this._stateManager.GetOrAddAsync<IReliableDictionary2<string, JobListDto>>(Consts.LIST_DICT);
            using (var tx = this._stateManager.CreateTransaction())
            {
                await list_dict.AddAsync(tx, key, new JobListDto()
                {
                    Key = key,
                    Value = value
                });
                await tx.CommitAsync();
            }
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

        public async Task RemoveRange(string key, int keepStartingFrom, int keepEndingAt)
        {
            var list_dict = await this._stateManager.GetOrAddAsync<IReliableDictionary2<string, JobListDto>>(Consts.LIST_DICT);
            using (var tx = this._stateManager.CreateTransaction())
            {
                var emulator = (await list_dict.CreateEnumerableAsync(tx)).GetAsyncEnumerator();
                var ls = new List<JobListDto>();
                while (await emulator.MoveNextAsync(default))
                {
                    ls.Add(emulator.Current.Value);
                }
                var removed = ls.OrderBy(u => u.Key).Skip(keepEndingAt).Take(keepEndingAt - keepStartingFrom).ToList();
                foreach (var re in removed)
                {
                    await list_dict.TryRemoveAsync(tx, re.Key);
                }
                await tx.CommitAsync();
            }
        }
    }
}
