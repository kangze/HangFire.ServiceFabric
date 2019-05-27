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
        Task<JobDto> AddJobAsync(JobDto job);

        Task<JobDto> GetJobAsync(long JobId);

        Task<List<JobDto>> GetAllJobsAsync();

        Task UpdateJobAsync(JobDto job);

        Task<int> GetNumberbyStateName(string stateName);
    }
}
