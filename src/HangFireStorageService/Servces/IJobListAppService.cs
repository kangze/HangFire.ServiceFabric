using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HangFireStorageService.Servces
{
    public interface IJobListAppService : IService
    {
        Task RemoveRange(string key, int keepStartingFrom, int keepEndingAt);

        Task AddAsync(string key, string value);

        Task Remove(string key, string value);
    }
}
