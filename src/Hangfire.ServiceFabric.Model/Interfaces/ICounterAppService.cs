using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire.ServiceFabric.Model.Dtos;
using Microsoft.ServiceFabric.Services.Remoting;

namespace Hangfire.ServiceFabric.Model.Interfaces
{
    public interface ICounterAppService : IService
    {
        Task<List<CounterDto>> GetAllCounterAsync();

        Task AddAsync(string key, TimeSpan? expireIn);

        Task DecrementAsync(string key, long amount, TimeSpan? expireIn);

        Task<CounterDto> GetCounterAsync(string key);
    }
}
