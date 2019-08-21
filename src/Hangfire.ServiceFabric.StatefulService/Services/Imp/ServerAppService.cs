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

            var server_dict = await GetServerDtosDictAsync();

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

                await tx.CommitAsync();
            }


        }

        public async Task<List<ServerDtos>> GetAllServerAsync()
        {
            var server_dict = await GetServerDtosDictAsync();
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

            var server_dict = await GetServerDtosDictAsync();
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
            var server_dict = await GetServerDtosDictAsync();
            using (var tx = this._stateManager.CreateTransaction())
            {
                await server_dict.TryRemoveAsync(tx, serverId);
                await tx.CommitAsync();
            }
        }

        private async Task<IReliableDictionary2<string, ServerDtos>> GetServerDtosDictAsync()
        {
            var server_dict = await this._stateManager.GetOrAddAsync<IReliableDictionary2<string, ServerDtos>>(Consts.SERVER_DICT);
            return server_dict;
        }
    }
}
