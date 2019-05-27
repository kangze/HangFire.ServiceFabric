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
        /// <summary>
        /// 获取所有的JobQueues
        /// </summary>
        /// <returns></returns>
        Task<List<JobQuequeDto>> GetAllJobQueueusAsync();
    }
}
