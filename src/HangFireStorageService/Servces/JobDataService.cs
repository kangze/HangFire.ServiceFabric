using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HangFireStorageService.Dto;
using Microsoft.ServiceFabric.Data;

namespace HangFireStorageService.Servces
{
    public class JobDataService : IJobDataService
    {
        private readonly IReliableStateManager _stateManager;



        public JobDataService(IReliableStateManager stateManager)
        {
            _stateManager = stateManager;
        }

        public Task UpdateJobAsync(List<OperationDto> operations)
        {
            if (operations == null)
                throw new NullReferenceException(nameof(operations));
            foreach (var operation in operations)
            {
                switch (operation.OperationType)
                {
                    case OperationEnum.AddJobState:
                        break;
                }
            }
        }
    }
}
