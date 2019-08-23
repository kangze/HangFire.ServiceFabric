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
    public class ServerAppService : IServerAppService
    {
        private readonly IReliableStateManager _stateManager;
        private readonly string _dictName;

        public ServerAppService(IReliableStateManager stateManager, string dictName)
        {
            this._stateManager = stateManager;
            _dictName = dictName;
        }

        public async Task AddOrUpdateAsync(string serverId, string data, DateTime heartBeat)
        {
            if (string.IsNullOrEmpty(serverId))
                throw new ArgumentNullException(nameof(serverId));
            if (string.IsNullOrEmpty(data))
                throw new ArgumentNullException(nameof(data));

            var serverDict = await StoreFactory.CreateServerDictAsync(this._stateManager, this._dictName);

            using (var tx = this._stateManager.CreateTransaction())
            {
                var existedCondition = await serverDict.TryGetValueAsync(tx, serverId);
                if (existedCondition.HasValue)
                {
                    existedCondition.Value.Data = data;
                    existedCondition.Value.LastHeartbeat = heartBeat;
                    await serverDict.AddOrUpdateAsync(tx, serverId, existedCondition.Value, (k, v) => existedCondition.Value);
                }
                else
                {
                    var serverDto = new ServerDtos()
                    {
                        Data = data,
                        ServerId = serverId,
                        LastHeartbeat = heartBeat
                    };
                    await serverDict.TryAddAsync(tx, serverId, serverDto);
                }

                await tx.CommitAsync();
            }


        }

        public async Task<List<ServerDtos>> GetAllServerAsync()
        {
            var serverDict = await StoreFactory.CreateServerDictAsync(this._stateManager, this._dictName);
            using (var tx = this._stateManager.CreateTransaction())
            {
                var ls = new List<ServerDtos>();
                var enumerator = (await serverDict.CreateEnumerableAsync(tx)).GetAsyncEnumerator();
                while (await enumerator.MoveNextAsync(default))
                {
                    ls.Add(enumerator.Current.Value);
                }
                return ls;
            }
        }

        public async Task<ServerDtos> GetServerAsync(string serverId)
        {
            if (string.IsNullOrEmpty(serverId))
                throw new ArgumentNullException(nameof(serverId));

            var serverDict = await StoreFactory.CreateServerDictAsync(this._stateManager, this._dictName);
            using (var tx = this._stateManager.CreateTransaction())
            {
                var serverCondition = await serverDict.TryGetValueAsync(tx, serverId);
                return serverCondition.HasValue ? serverCondition.Value : null;
            }
        }

        public async Task RemoveServer(string serverId)
        {
            if (string.IsNullOrEmpty(serverId))
                throw new ArgumentNullException(nameof(serverId));
            var serverDict = await StoreFactory.CreateServerDictAsync(this._stateManager, this._dictName);
            using (var tx = this._stateManager.CreateTransaction())
            {
                await serverDict.TryRemoveAsync(tx, serverId);
                await tx.CommitAsync();
            }
        }
    }
}
