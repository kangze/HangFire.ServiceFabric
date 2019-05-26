using Hangfire.Storage.Monitoring;
using HangFireStorageService.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HangFireStorageService.Servces
{
    public interface IServerAppService
    {
        Task AddOrUpdateAsync(string serverId, string data, DateTimeOffset heartBeat);

        Task<ServerDtos> GetServerAsync(string serverId);

        Task<List<ServerDtos>> GetAllServerAsync();

        Task RemoveServer(string serverId);
    }
}
