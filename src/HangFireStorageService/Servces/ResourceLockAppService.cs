using HangFireStorageService;
using HangFireStorageService.Extensions;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.ServiceFabric.Servces
{
    public class ResourceLockAppService : IResourceLockAppService
    {
        private readonly IReliableStateManager _stateManager;
        private readonly ServiceFabricOptions _options;

        public ResourceLockAppService(IReliableStateManager stateManager, ServiceFabricOptions options)
        {
            this._stateManager = stateManager;
            this._options = options;
        }

        public async Task<bool> LockAsync(string resource)
        {
            var lock_dict = await this._stateManager.GetOrAddAsync<IReliableDictionary2<string, int>>(string.Format(Consts.LOCK_DICT, this._options.Prefix));
            using (var tx = this._stateManager.CreateTransaction())
            {
                var lock_condition = await lock_dict.TryGetValueAsync(tx, resource);
                if (lock_condition.HasValue)
                {
                    await lock_dict.SetAsync(tx, resource, lock_condition.Value + 1);
                }
                else
                {
                    await lock_dict.SetAsync(tx, resource, 1);
                }
                await tx.CommitAsync();
                return true;
            }
        }

        public async Task<bool> ReleaseAsync(string resource)
        {
            var lock_dict = await this._stateManager.GetOrAddAsync<IReliableDictionary2<string, int>>(string.Format(Consts.LOCK_DICT, this._options.Prefix));
            using (var tx = this._stateManager.CreateTransaction())
            {
                var lock_condition = await lock_dict.TryGetValueAsync(tx, resource);
                var result = false;
                if (!lock_condition.HasValue)
                    result = true;
                else if (lock_condition.Value == 1)
                {
                    await lock_dict.TryRemoveAsync(tx, resource);
                    result = true;
                }
                else if (lock_condition.Value > 1)
                {
                    await lock_dict.SetAsync(tx, resource, lock_condition.Value - 1);
                    result = false;
                }
                await tx.CommitAsync();
                return result;
            }
        }
    }
}