using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HangFireStorageService.Dto;
using Microsoft.ServiceFabric.Services.Remoting;

namespace HangFireStorageService.Servces
{
    public interface IJobDataService : IService
    {
        /// <summary>
        /// 获取Job的参数
        /// </summary>
        /// <param name="jobId"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        Task<string> GetJobParameter(string jobId, string name);

        Task AddOrUpdateJobParameter(string jobId, string name, string value);

        /// <summary>
        /// 更新操作
        /// </summary>
        /// <returns></returns>
        Task UpdateJobAsync(List<OperationDto> operations);

        /// <summary>
        /// 添加工作任务
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        Task<JobDto> AddJobAsync(JobDto dto);

        /// <summary>
        /// 获取一个Job任务
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        Task<JobDto> GetJobAsync(long jobId);
    }
}
