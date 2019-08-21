using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire.ServiceFabric.Model.Dtos;
using Microsoft.ServiceFabric.Services.Remoting;

namespace Hangfire.ServiceFabric.Model.Interfaces
{
    public interface IListAppService : IService
    {
        Task RemoveRange(string key, int keepStartingFrom, int keepEndingAt);

        Task AddAsync(string key, string value);

        Task Remove(string key, string value);

        Task<List<ListDto>> GetListDtoAsync(string key);
    }
}
