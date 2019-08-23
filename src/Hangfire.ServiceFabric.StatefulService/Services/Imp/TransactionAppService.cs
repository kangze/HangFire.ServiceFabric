using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire.ServiceFabric.Model.Dtos;
using Hangfire.ServiceFabric.Model.Interfaces;
using Hangfire.ServiceFabric.Model.TransactionOperations;
using Hangfire.ServiceFabric.Model.TransactionOperations.Arguments;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;

namespace Hangfire.ServiceFabric.StatefulService.Services.Imp
{
    public class TransactionAppService : ITransactionAppService
    {
        private readonly IReliableStateManager _stateManager;
        private readonly DictNames _dictNames;
        private readonly JobAppService _jobAppService;
        private readonly JobQueueAppService _jobQueueAppService;
        private readonly CounterAppService _counterAppService;
        private readonly JobSetAppService _setAppService;
        private readonly ListAppService _listAppService;
        private readonly HashAppService _hashAppService;


        private IReliableDictionary2<string, JobDto> _jobDtoDict;
        private IReliableDictionary2<string, JobQueueDto> _jobQueueDict;
        private IReliableDictionary2<string, CounterDto> _counterDict;
        private IReliableDictionary2<string, SetDto> _setDict;
        private IReliableDictionary2<string, ListDto> _listDict;
        private IReliableDictionary2<string, HashDto> _hashDict;

        public TransactionAppService(IReliableStateManager stateManager, DictNames dictNames)
        {
            _stateManager = stateManager;
            _dictNames = dictNames;
            _jobAppService = new JobAppService(stateManager, dictNames.JobDictName);
            _jobQueueAppService = new JobQueueAppService(stateManager, dictNames.JobQueueDictName);
            _counterAppService = new CounterAppService(stateManager, dictNames.CounterDictName);
            _setAppService = new JobSetAppService(stateManager, dictNames.SetDictName);
            _listAppService = new ListAppService(stateManager, dictNames.ListDictName);
            _hashAppService = new HashAppService(stateManager, dictNames.HashDictName);
        }


        public async Task CommitAsync(List<Operation> operations)
        {
            await InitDataStoresAsync();

            //impl Transaction
            using (var tx = this._stateManager.CreateTransaction())
            {
                foreach (var operation in operations)
                {
                    switch (operation.OperationType)
                    {
                        case OperationType.ExpireJob:
                        case OperationType.SetJobParameter:
                        case OperationType.PersistJob:
                        case OperationType.SetJobState:
                        case OperationType.AddJobState:
                            var arg = operation.GetArguments<JobArg>();
                            await this._jobAppService.AddOrUpdateAsync(tx, this._jobDtoDict, arg.JobId,
                                job =>
                                {
                                    UpdateJob(job, operation);
                                    return job;
                                });
                            break;
                        case OperationType.CreateExpiredJob:
                            var arg1 = operation.GetArguments<CreateExpiredJobArg>();
                            await this._jobAppService.AddOrUpdateAsync(tx, this._jobDtoDict, arg1.JobDto.Id,
                                job => arg1.JobDto);
                            break;
                        case OperationType.AddToQueue:
                            var arg6 = operation.GetArguments<AddToQueueArg>();
                            await this._jobQueueAppService.AddAsync(tx, this._jobQueueDict, arg6.Id, arg6.JobId, arg6.Queue);
                            break;
                        case OperationType.IncrementCounter:
                        case OperationType.DecrementCounter:
                            var arg7 = operation.GetArguments<IncrementCounterArg>();
                            await this._counterAppService.AddAsync(tx, this._counterDict, new CounterDto()
                            {
                                Id = Guid.NewGuid().ToString("N"),
                                Key = arg7.Key,
                                Value = arg7.Value,
                                ExpireAt = arg7.ExpireIn.HasValue ? (DateTime?)DateTime.UtcNow.Add(arg7.ExpireIn.Value) : null,
                            });
                            break;
                        case OperationType.AddToSet:
                            var arg8 = operation.GetArguments<AddToSetArg>();
                            await this._setAppService.AddSetAsync(tx, this._setDict, new SetDto()
                            {
                                Key = arg8.Key,
                                Value = arg8.Value,
                                Score = arg8.Score
                            });
                            break;
                        case OperationType.RemoveFromSet:
                            var arg9 = operation.GetArguments<RemoveFromSetArg>();
                            await this._setAppService.RemoveAsync(tx, this._setDict, new SetDto()
                            {
                                Key = arg9.Key,
                                Value = arg9.Value,
                            });
                            break;
                        case OperationType.InsertToList:
                            var arg10 = operation.GetArguments<InsertToListArg>();
                            await this._listAppService.AddAsync(tx, this._listDict, new ListDto()
                            {
                                Id = Guid.NewGuid().ToString("N"),
                                Item = arg10.Key,
                                Value = arg10.Value
                            });
                            break;
                        case OperationType.RemoveFromList:
                            var arg11 = operation.GetArguments<RemoveFromListArg>();
                            await this._listAppService.Remove(tx, this._listDict, new ListDto()
                            {
                                Item = arg11.Key,
                                Value = arg11.Value
                            });
                            break;
                        case OperationType.TrimList:
                            var arg12 = operation.GetArguments<TrimListArg>();
                            await this._listAppService.RemoveRange(tx, this._listDict, arg12.Key, arg12.KeepStartingFrom, arg12.KeepEndingAt);
                            break;
                        case OperationType.SetRangeInHash:
                            var arg13 = operation.GetArguments<SetRangInHashArg>();
                            await this._hashAppService.SetRangInHash(tx, this._hashDict, arg13.Key,
                                arg13.KeyValuePairs);
                            break;
                        case OperationType.RemoveHash:
                            var arg14 = operation.GetArguments<RemoveHashArg>();
                            await this._hashAppService.RemoveAsync(tx, this._hashDict, arg14.Key);
                            break;

                    }
                }

                try
                {
                    await tx.CommitAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }

            }

        }

        private void UpdateJob(JobDto job, Operation operation)
        {
            switch (operation.OperationType)
            {
                case OperationType.ExpireJob:
                    var arg0 = operation.GetArguments<ExpireJobArg>();
                    job.ExpireAt = DateTime.UtcNow.Add(arg0.ExpireIn);
                    break;
                case OperationType.SetJobParameter:
                    var arg2 = operation.GetArguments<SetJobParameterArg>();
                    job.Parameters.AddOrUpdate(arg2.Name, arg2.Value);
                    break;
                case OperationType.PersistJob:
                    var arg3 = operation.GetArguments<PersistJobArg>();
                    job.ExpireAt = null;
                    break;
                case OperationType.SetJobState:
                    var arg4 = operation.GetArguments<SetJobStateArg>();
                    job.StateHistory.Add(arg4.StateDto);
                    job.StateName = arg4.StateDto.Name;
                    break;
                case OperationType.AddJobState:
                    var arg5 = operation.GetArguments<AddJobStateArg>();
                    job.StateHistory.Add(arg5.StateDto);
                    break;
            }
        }

        private async Task InitDataStoresAsync()
        {
            this._jobDtoDict = await StoreFactory.CreateJobDictAsync(this._stateManager, this._dictNames.JobDictName);
            this._jobQueueDict = await StoreFactory.CreateJobQueueDictAsync(this._stateManager, this._dictNames.JobQueueDictName);
            this._counterDict = await StoreFactory.CreateCounterDictAsync(this._stateManager, this._dictNames.CounterDictName);
            this._setDict = await StoreFactory.CreateSetDictAsync(this._stateManager, this._dictNames.SetDictName);
            this._listDict = await StoreFactory.CreateListDictAsync(this._stateManager, this._dictNames.ListDictName);
            this._hashDict = await StoreFactory.CreateHashDictAsync(this._stateManager, this._dictNames.HashDictName);
        }
    }
}
