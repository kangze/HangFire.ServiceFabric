using Hangfire.Storage.Monitoring;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HangFireStorageService.Servces
{
    public interface IServerAppService
    {
        Task AnnounceServerAsync(string serverId, string data, DateTimeOffset heartBeat);

        Task<ServerDtos> GetServerAsync(string serverId);
    }
}
