using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire.ServiceFabric.Model.Dtos;
using Microsoft.ServiceFabric.Services.Remoting;

namespace Hangfire.ServiceFabric.Model.Interfaces
{
    public interface IHashAppService : IService
    {
        Task RemoveAsync(string key);

        Task<HashDto> GetHashDtoAsync(string key);

        Task<List<HashDto>> GetAllHashAsync();

        Task AddOrUpdateAsync(HashDto dto);
    }
}
