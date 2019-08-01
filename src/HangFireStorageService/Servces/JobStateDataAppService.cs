using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Hangfire.ServiceFabric.Dtos;
using Hangfire.ServiceFabric.Entities;
using Hangfire.ServiceFabric.Servces;
using HangFireStorageService.Dto;
using HangFireStorageService.Extensions;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;

namespace HangFireStorageService.Servces
{
    public class JobStateDataAppService : HangfireDataBaseService, IJobStateDataAppService
    {

        public JobStateDataAppService(IReliableStateManager stateManager, ServiceFabricOptions options)
            : base(stateManager, options)
        {

        }

        public async Task<List<StateDto>> GetAllStateAsync()
        {
            var ls = new List<StateDto>();
            var stateDict = await this._stateManager.GetOrAddAsync<IReliableDictionary2<string, StateDto>>(string.Format(Consts.STATE_DICT, this._options.Prefix));
            using (var tx = this._stateManager.CreateTransaction())
            {
                var enumlator = (await stateDict.CreateEnumerableAsync(tx)).GetAsyncEnumerator();
                while (await enumlator.MoveNextAsync(default))
                {
                    ls.Add(enumlator.Current.Value);
                }
                return ls;
            }
        }

        public async Task<StateDto> GetLatestJobStateDataAsync(string jobId)
        {
            var job_dict = await this._stateManager.GetOrAddAsync<IReliableDictionary2<string, JobEntity>>(string.Format(Consts.JOB_DICT, this._options.Prefix));
            var state_dict = await this._stateManager.GetOrAddAsync<IReliableDictionary2<string, StateEntity>>(string.Format(Consts.STATE_DICT, this._options.Prefix));
            using (var tx = this._stateManager.CreateTransaction())
            {
                var job_condition = await job_dict.TryGetValueAsync(tx, jobId);
                if (!job_condition.HasValue)
                    return null;
                var state_condition = await state_dict.TryGetValueAsync(tx, job_condition.Value.StateId);
                if (!state_condition.HasValue)
                    return null;
                return this._mapper.Map<StateDto>(state_condition.Value);
            }
        }

        public async Task<List<StateDto>> GetStates(string jobId)
        {
            var state_dict = await this._stateManager.GetOrAddAsync<IReliableDictionary2<string, StateEntity>>(string.Format(Consts.STATE_DICT, this._options.Prefix));
            using (var tx = this._stateManager.CreateTransaction())
            {
                var ls = new List<StateDto>();
                var enumlator = (await state_dict.CreateEnumerableAsync(tx)).GetAsyncEnumerator();
                while (await enumlator.MoveNextAsync(default))
                {
                    if (enumlator.Current.Value.JobId == jobId)
                        ls.Add(this._mapper.Map<StateDto>(enumlator.Current.Value));
                }
                return ls;
            }
        }

        public async Task AddStateAsync(string jobId, StateDto state)
        {
            var state_dict = await this._stateManager.GetOrAddAsync<IReliableDictionary2<string, StateEntity>>(Consts.STATE_DICT);
            using (var tx = this._stateManager.CreateTransaction())
            {
                var stateId = Guid.NewGuid().ToString();
                await state_dict.SetAsync(tx, stateId, this._mapper.Map<StateEntity>(state));
                await tx.CommitAsync();
            }
        }
    }
}
