using HangFireStorageService.Dto;
using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HangFireStorageService.Servces
{
    public interface IHashAppService : IService
    {
        Task RemoveAsync(string key);

        Task<HashDto> GetHashDtoAsync(string key);

        Task<List<HashDto>> GetAllHashAsync();

        Task AddOrUpdateAsync(HashDto dto);
    }
}
