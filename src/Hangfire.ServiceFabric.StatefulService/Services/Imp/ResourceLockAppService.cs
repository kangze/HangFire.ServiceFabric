using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire.ServiceFabric.Extensions;
using Hangfire.ServiceFabric.Model;
using Hangfire.ServiceFabric.Model.Interfaces;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;

namespace Hangfire.ServiceFabric.StatefulService.Services.Imp
{

    public class ResourceLockAppService : IResourceLockAppService
    {
        private readonly IReliableStateManager _stateManager;
        private readonly string _dictName;

        public ResourceLockAppService(IReliableStateManager stateManager, string dictName)
        {
            this._stateManager = stateManager;
            _dictName = dictName;
        }

        public async Task<bool> LockAsync(string resource)
        {
            var lock_dict = await this._stateManager.GetOrAddAsync<IReliableDictionary2<string, int>>(Consts.LOCK_DICT);
            using (var tx = this._stateManager.CreateTransaction())
            {
                var lock_condition = await lock_dict.TryGetValueAsync(tx, resource);
                if (lock_condition.HasValue) return false;
                try
                {
                    await lock_dict.SetAsync(tx, resource, 0);
                    await tx.CommitAsync();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }

            }
        }

        public async Task<bool> ReleaseAsync(string resource)
        {
            var lock_dict = await this._stateManager.GetOrAddAsync<IReliableDictionary2<string, int>>(Consts.LOCK_DICT);
            using (var tx = this._stateManager.CreateTransaction())
            {
                var lock_condition = await lock_dict.TryGetValueAsync(tx, resource);
                if (!lock_condition.HasValue)
                    return true;
                await lock_dict.TryRemoveAsync(tx, resource);
                await tx.CommitAsync();
                return true;
            }
        }
    }
}