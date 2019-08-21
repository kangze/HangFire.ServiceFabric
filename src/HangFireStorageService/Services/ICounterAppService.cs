using Hangfire.ServiceFabric.Dtos;
using HangFireStorageService.Dto;
using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.ServiceFabric.Services
{
    public interface ICounterAppService : IService
    {
        Task<List<CounterDto>> GetAllCounterAsync();

        Task AddAsync(string key, TimeSpan? expireIn);

        Task DecrementAsync(string key, long amount, TimeSpan? expireIn);

        Task<CounterDto> GetCounterAsync(string key);
    }
}
