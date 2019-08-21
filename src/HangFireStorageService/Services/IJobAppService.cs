using Hangfire.ServiceFabric.Dtos;
using HangFireStorageService.Dto;
using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.ServiceFabric.Services
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
