using Hangfire.ServiceFabric.Dtos;
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
        Task<StateDto> GetLatestJobStateDataAsync(string jobId);

        Task<List<StateDto>> GetAllStateAsync();

        Task<List<StateDto>> GetStates(string jobId);

        Task AddStateAsync(string jobId, StateDto state);



    }
}
