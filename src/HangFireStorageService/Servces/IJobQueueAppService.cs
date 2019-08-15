﻿using Hangfire.ServiceFabric.Dtos;
using Hangfire.ServiceFabric.Entities;
using HangFireStorageService.Dto;
using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.ServiceFabric.Servces
{
    public interface IJobQueueAppService : IService
    {
        Task<List<JobQueueDto>> GetQueuesAsync(string queue);

        Task<JobQueueDto> GetFetchedJobAsync(string queue);

        Task<JobQueueDto> GetQueueAsync(string id);

        Task DeleteQueueJobAsync(string id);

        Task UpdateQueueAsync(JobQueueDto dto);

        Task AddToQueueJObAsync(string queue, string jobId);

        Task<List<JobQueueDto>> GetEnqueuedJob(string queue, int from, int perPage);

        Task<List<string>> GetFetchedJobIds(string queue, int from, int perPage);

        Task<EnqueuedAndFetchedCountDto> GetEnqueuedAndFetchedCount(string queue);


    }
}
