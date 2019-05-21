using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HangFireStorageService.Dto;
using HangFireStorageService.Dto.Internal;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;

namespace HangFireStorageService.Servces
{
    public class JobDataService : IJobDataService
    {

        private readonly IReliableStateManager _stateManager;
        private JobAndStateProccesser _jobAndstateProccesser;

        public const string Job = "HangFire.ServiceFabric.Job";
        public const string State = "HangFire.ServiceFabric.State";


        public JobDataService(IReliableStateManager stateManager)
        {
            _stateManager = stateManager;
        }

        public async Task UpdateJobAsync(List<OperationDto> operations)
        {
            if (operations == null)
                throw new NullReferenceException(nameof(operations));
            using (var tx = _stateManager.CreateTransaction())
            {
                await this.BuildPrccesserAsync(tx);
                foreach (var operation in operations)
                {
                    switch (operation.OperationType)
                    {
                        case OperationEnum.AddJobState:
                            var addJobState_arg = operation.Argument as AddJobStateArgument;
                            await this._jobAndstateProccesser.Proccess_AddJobState(addJobState_arg.JobId, addJobState_arg.State);
                            break;
                        case OperationEnum.SetJobState:
                            var setJobState_arg = operation.Argument as SetJobStateArgument;
                            await this._jobAndstateProccesser.Proccess_SetJobState(setJobState_arg.JobId, setJobState_arg.State);
                            break;
                        case OperationEnum.Expire:
                            var expireJob_arg = operation.Argument as ExpireJobArgument;
                            await this._jobAndstateProccesser.Prccess_ExpireJob(expireJob_arg.JobId, expireJob_arg.ExpireIn);
                            break;
                        case OperationEnum.Persist:
                            var persist_arg = operation.Argument as PersistJobArgument;
                            await this._jobAndstateProccesser.Proccess_PersistJob(persist_arg.JobId);
                            break;
                    }
                }

                await tx.CommitAsync();
            }
        }

        private async Task BuildPrccesserAsync(ITransaction tx)
        {
            var states = await this._stateManager.GetOrAddAsync<IReliableDictionary2<long, List<StateDto>>>(State);
            var jobs = await this._stateManager.GetOrAddAsync<IReliableDictionary2<long, JobDto>>(Job);

            _jobAndstateProccesser = new JobAndStateProccesser(tx, states, jobs);
        }

        public async Task<JobDto> AddJobAsync(JobDto dto)
        {
            return new JobDto();
        }

        public async Task<JobDto> GetJobAsync(long jobId)
        {
            return new JobDto();
        }
    }
}
