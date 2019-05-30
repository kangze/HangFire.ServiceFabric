using HangFireStorageService.Dto;
using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HangFireStorageService.Servces
{
    public interface IJobStateDataAppService : IService
    {
        Task<StateDto> GetLatestJobStateDataAsync(long jobId);

        Task<List<StateDto>> GetAllStateAsync();

        Task<List<StateDto>> GetStates(long jobId);

        Task AddStateAsync(long jobId, StateDto state);



    }
}
