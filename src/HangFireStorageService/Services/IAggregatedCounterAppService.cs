using HangFireStorageService.Dto;
using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.ServiceFabric.Services
{
    public interface IAggregatedCounterAppService : IService
    {
        Task<List<AggregatedCounterDto>> GetAllCounterAsync();
    }
}
