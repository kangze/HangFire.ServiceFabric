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
        private readonly string _dictName;
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

        public TransactionAppService(IReliableStateManager stateManager, string dictName)
        {
            _stateManager = stateManager;
            _dictName = dictName;
            _jobAppService = new JobAppService(stateManager, dictName);
            _jobQueueAppService = new JobQueueAppService(stateManager, dictName);
            _counterAppService = new CounterAppService(stateManager, dictName);
            _setAppService = new JobSetAppService(stateManager, dictName);
            _listAppService = new ListAppService(stateManager, dictName);
            _hashAppService = new HashAppService(stateManager, dictName);
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
                            var arg0 = operation.GetArguments<ExpireJobArg>(operation.Arg);
                            await this._jobAppService.AddOrUpdateAsync(tx, this._jobDtoDict, arg0.JobId, job =>
                                {
                                    job.ExpireAt = DateTime.UtcNow.Add(arg0.ExpireIn);
                                    return job;
                                });
                            break;
                        case OperationType.CreateExpiredJob:
                            var arg1 = operation.GetArguments<CreateExpiredJobArg>(operation.Arg);
                            await this._jobAppService.AddOrUpdateAsync(tx, this._jobDtoDict, arg1.JobDto.Id,
                                job => arg1.JobDto);
                            break;
                        case OperationType.SetJobParameter:
                            var arg2 = operation.GetArguments<SetJobParameterArg>(operation.Arg);
                            await this._jobAppService.AddOrUpdateAsync(tx, this._jobDtoDict, arg2.JobId, job =>
                            {
                                job.Parameters.AddOrUpdate(arg2.Name, arg2.Value);
                                return job;
                            });
                            break;
                        case OperationType.PersistJob:
                            var arg3 = operation.GetArguments<PersistJobArg>(operation.Arg);
                            await this._jobAppService.AddOrUpdateAsync(tx, this._jobDtoDict, arg3.JobId, job =>
                            {
                                job.ExpireAt = null;
                                return job;
                            });
                            break;
                        case OperationType.SetJobState:
                            var arg4 = operation.GetArguments<SetJobStateArg>(operation.Arg);
                            await this._jobAppService.AddOrUpdateAsync(tx, this._jobDtoDict, arg4.JobId, job =>
                            {
                                job.StateHistory.Add(arg4.StateDto);
                                job.StateName = arg4.StateDto.Name;
                                return job;
                            });
                            break;
                        case OperationType.AddJobState:
                            var arg5 = operation.GetArguments<SetJobStateArg>(operation.Arg);
                            await this._jobAppService.AddOrUpdateAsync(tx, this._jobDtoDict, arg5.JobId, job =>
                            {
                                job.StateHistory.Add(arg5.StateDto);
                                return job;
                            });
                            break;
                        case OperationType.AddToQueue:
                            var arg6 = operation.GetArguments<AddToQueueArg>(operation.Arg);
                            await this._jobQueueAppService.AddAsync(tx, this._jobQueueDict, arg6.Id, arg6.JobId, arg6.Queue);
                            break;
                        case OperationType.IncrementCounter:
                        case OperationType.DecrementCounter:
                            var arg7 = operation.GetArguments<IncrementCounterArg>(operation.Arg);
                            await this._counterAppService.AddAsync(tx, this._counterDict, new CounterDto()
                            {
                                Id = Guid.NewGuid().ToString("N"),
                                Key = arg7.Key,
                                Value = arg7.Value,
                                ExpireAt = arg7.ExpireIn.HasValue ? (DateTime?)DateTime.UtcNow.Add(arg7.ExpireIn.Value) : null,
                            });
                            break;
                        case OperationType.AddToSet:
                            var arg8 = operation.GetArguments<AddToSetArg>(operation.Arg);
                            await this._setAppService.AddSetAsync(tx, this._setDict, new SetDto()
                            {
                                Key = arg8.Key,
                                Value = arg8.Value,
                                Score = arg8.Score
                            });
                            break;
                        case OperationType.RemoveFromSet:
                            var arg9 = operation.GetArguments<RemoveFromSetArg>(operation.Arg);
                            await this._setAppService.AddSetAsync(tx, this._setDict, new SetDto()
                            {
                                Key = arg9.Key,
                                Value = arg9.Value,
                            });
                            break;
                        case OperationType.InsertToList:
                            var arg10 = operation.GetArguments<InsertToListArg>(operation.Arg);
                            await this._listAppService.AddAsync(tx, this._listDict, new ListDto()
                            {
                                Id = Guid.NewGuid().ToString("N"),
                                Item = arg10.Key,
                                Value = arg10.Value
                            });
                            break;
                        case OperationType.RemoveFromList:
                            var arg11 = operation.GetArguments<RemoveFromListArg>(operation.Arg);
                            await this._listAppService.AddAsync(tx, this._listDict, new ListDto()
                            {
                                Item = arg11.Key,
                                Value = arg11.Value
                            });
                            break;
                        case OperationType.TrimList:
                            var arg12 = operation.GetArguments<TrimListArg>(operation.Arg);
                            await this._listAppService.RemoveRange(tx, this._listDict, arg12.Key, arg12.KeepStartingFrom, arg12.KeepEndingAt);
                            break;
                        case OperationType.SetRangeInHash:
                            var arg13 = operation.GetArguments<SetRangInHashArg>(operation.Arg);
                            await this._hashAppService.SetRangInHash(tx, this._hashDict, arg13.Key,
                                arg13.KeyValuePairs);
                            break;
                        case OperationType.RemoveHash:
                            var arg14 = operation.GetArguments<RemoveHashArg>(operation.Arg);
                            await this._hashAppService.RemoveAsync(tx, this._hashDict, arg14.Key);
                            break;

                    }
                }

                await tx.CommitAsync();
            }

        }

        private async Task InitDataStoresAsync()
        {
            this._jobDtoDict = await StoreFactory.CreateJobDictAsync(this._stateManager, this._dictName);
            this._jobQueueDict = await StoreFactory.CreateJobQueueDictAsync(this._stateManager, this._dictName);
            this._counterDict = await StoreFactory.CreateCounterDictAsync(this._stateManager, this._dictName);
            this._setDict = await StoreFactory.CreateSetDictAsync(this._stateManager, this._dictName);
            this._listDict = await StoreFactory.CreateListDictAsync(this._stateManager, this._dictName);
            this._hashDict = await StoreFactory.CreateHashDictAsync(this._stateManager, this._dictName);
        }
    }
}
