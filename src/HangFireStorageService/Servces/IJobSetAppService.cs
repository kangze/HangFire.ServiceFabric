using Hangfire.ServiceFabric.Dtos;
using HangFireStorageService.Dto;
using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HangFireStorageService.Servces
{
    public interface IJobSetAppService : IService
    {
        Task<List<SetDto>> GetSetsAsync();

        Task AddSetAsync(string key, string value, double score);

        Task RemoveAsync(string key, string value);

    }
}
