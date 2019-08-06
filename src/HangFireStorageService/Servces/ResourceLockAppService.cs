﻿using HangFireStorageService;
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
    /// <summary>
    /// DistruibuteLock Imp,If throw Timeout Exception,beacause of gain lock failed
    /// </summary>
    public class ResourceLockAppService : IResourceLockAppService
    {
        private readonly IReliableStateManager _stateManager;

        public ResourceLockAppService(IReliableStateManager stateManager)
        {
            this._stateManager = stateManager;
        }

        public async Task<bool> LockAsync(string resource)
        {
            var lock_dict = await this._stateManager.GetOrAddAsync<IReliableDictionary2<string, int>>(Consts.LOCK_DICT);
            using (var tx = this._stateManager.CreateTransaction())
            {
                var lock_condition = await lock_dict.TryGetValueAsync(tx, resource);
                if (lock_condition.HasValue)
                {
                    return false;
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
            var lock_dict = await this._stateManager.GetOrAddAsync<IReliableDictionary2<string, int>>(Consts.LOCK_DICT);
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