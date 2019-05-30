using HangFireStorageService.Dto;
using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HangFireStorageService.Servces
{
    public interface ICounterAppService : IService
    {
        Task<List<CounterDto>> GetAllCounterAsync();

        Task AddAsync(string key, TimeSpan? expireIn);

        Task DeleteAsync(string key, TimeSpan? expireIn);
    }
}
