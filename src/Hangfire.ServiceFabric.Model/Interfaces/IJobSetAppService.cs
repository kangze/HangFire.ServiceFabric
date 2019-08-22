using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire.ServiceFabric.Model.Dtos;
using Microsoft.ServiceFabric.Services.Remoting;

namespace Hangfire.ServiceFabric.Model.Interfaces
{
    public interface IJobSetAppService : IService
    {
        Task<List<SetDto>> GetSetsAsync();
    }
}
