using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Hangfire.ServiceFabric.Model.Dtos;
using Microsoft.ServiceFabric.Services.Remoting;

namespace Hangfire.ServiceFabric.Model.Interfaces
{
    public interface IJobAppService : IService
    {
        Task<JobDto> AddOrUpdateAsync(JobDto jobDto);

        Task<JobDto> GetJobAsync(string JobId);

        Task<List<JobDto>> GetJobsByStateNameAsync(string stateName);

        Task<List<JobDto>> GetJobsByIdsAsync(string[] jobIds);

        Task<List<JobDto>> GetJobsAsync();

    }
}
