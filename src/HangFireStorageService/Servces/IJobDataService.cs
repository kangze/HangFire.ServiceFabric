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
        /// 更新操作
        /// </summary>
        /// <returns></returns>
        Task UpdateJobAsync(List<OperationDto> operations);
    }
}
