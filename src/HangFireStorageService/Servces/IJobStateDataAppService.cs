using HangFireStorageService.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HangFireStorageService.Servces
{
    public interface IJobStateDataAppService
    {
        Task<StateDto> GetLatestJobStateDataAsync(long jobId);

        Task<List<StateDto>> GetAllStateAsync();

        Task<List<StateDto>> GetStates(long jobId);

        
    }
}
