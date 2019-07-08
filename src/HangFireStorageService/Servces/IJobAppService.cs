using Hangfire.ServiceFabric.Dtos;
using HangFireStorageService.Dto;
using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HangFireStorageService.Servces
{
    public interface IJobAppService : IService
    {
        Task<JobDto> AddOrUpdateAsync(JobDto jobDto);

        Task<List<JobDto>> GetJobsAsync(string JobId);

        Task<List<JobDto>> GetJobsByStateNameAsync(string stateName);

        Task<List<JobDto>> GetJobDetailsAsync(string[] jobIds);

        Task AddJobStateAsync(string jobId, StateDto state);


    }
}
