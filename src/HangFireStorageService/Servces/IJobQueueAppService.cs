using HangFireStorageService.Dto;
using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HangFireStorageService.Servces
{
    public interface IJobQueueAppService : IService
    {
        Task<List<JobQueueDto>> GetQueuesAsync(string queue);

        Task DeleteQueueJobAsync(string queue, long jobId);

        Task AddToQueueJObAsync(string queue, long jobId);

    }
}
