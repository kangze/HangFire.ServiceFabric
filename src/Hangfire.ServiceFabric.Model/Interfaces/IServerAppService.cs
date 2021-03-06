﻿using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire.ServiceFabric.Model.Dtos;

namespace Hangfire.ServiceFabric.Model.Interfaces
{
    public interface IServerAppService:IService
    {
        Task AddOrUpdateAsync(string serverId, string data, DateTime heartBeat);

        Task<ServerDtos> GetServerAsync(string serverId);

        Task<List<ServerDtos>> GetAllServerAsync();

        Task RemoveServer(string serverId);
    }
}
