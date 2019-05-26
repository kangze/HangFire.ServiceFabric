using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire.Storage.Monitoring;
using HangFireStorageService.Dto;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;

namespace HangFireStorageService.Servces
{
    public class ServerAppService : IServerAppService
    {
        private readonly IReliableStateManager _stateManager;

        public ServerAppService(IReliableStateManager stateManager)
        {
            this._stateManager = stateManager;
        }

        public async Task AddOrUpdateAsync(string serverId, string data, DateTimeOffset heartBeat)
        {
            if (string.IsNullOrEmpty(serverId))
                throw new ArgumentNullException(nameof(serverId));
            if (string.IsNullOrEmpty(data))
                throw new ArgumentNullException(nameof(data));

            var server_dict = await this._stateManager.GetOrAddAsync<IReliableDictionary2<string, ServerDtos>>(Consts.SERVER_DICT);

            using (var tx = this._stateManager.CreateTransaction())
            {
                var existed_condition = await server_dict.TryGetValueAsync(tx, serverId);
                if (existed_condition.HasValue)
                {
                    existed_condition.Value.Data = data;
                    existed_condition.Value.LastHeartbeat = heartBeat;
                    await server_dict.AddOrUpdateAsync(tx, serverId, existed_condition.Value, (k, v) => existed_condition.Value);
                }
                else
                {
                    var serverDto = new ServerDtos()
                    {
                        Data = data,
                        ServerId = serverId,
                        LastHeartbeat = heartBeat
                    };
                    await server_dict.TryAddAsync(tx, serverId, serverDto);
                }
            }


        }

        public async Task<List<ServerDtos>> GetAllServerAsync()
        {
            var server_dict = await this._stateManager.GetOrAddAsync<IReliableDictionary2<string, ServerDtos>>(Consts.SERVER_DICT);
            using (var tx = this._stateManager.CreateTransaction())
            {
                var ls = new List<ServerDtos>();
                var enumerator = (await server_dict.CreateEnumerableAsync(tx)).GetAsyncEnumerator();
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

            var server_dict = await this._stateManager.GetOrAddAsync<IReliableDictionary2<string, ServerDtos>>(Consts.SERVER_DICT);
            using (var tx = this._stateManager.CreateTransaction())
            {
                var server_condition = await server_dict.TryGetValueAsync(tx, serverId);
                return server_condition.HasValue ? server_condition.Value : null;
            }
        }

        public async Task RemoveServer(string serverId)
        {
            if (string.IsNullOrEmpty(serverId))
                throw new ArgumentNullException(nameof(serverId));
            var server_dict = await this._stateManager.GetOrAddAsync<IReliableDictionary2<string, ServerDtos>>(Consts.SERVER_DICT);
            using (var tx = this._stateManager.CreateTransaction())
            {
                await server_dict.TryRemoveAsync(tx, serverId);
                await tx.CommitAsync();
            }
        }
    }
}
