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

        Task<JobDto> GetJobAsync(string jobId);

        Task<List<JobDto>> GetJobsByStateNameAsync(string stateName);

        Task<List<JobDto>> GetJobsByIdsAsync(string[] jobIds);

        Task<List<JobDto>> GetJobsAsync();

    }
}
