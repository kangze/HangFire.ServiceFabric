using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HangFireStorageService.Dto;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;

namespace HangFireStorageService.Servces
{
    public class JobStateDataAppService : IJobStateDataAppService
    {
        private readonly IReliableStateManager _stateManager;

        public JobStateDataAppService(IReliableStateManager stateManager)
        {
            this._stateManager = stateManager;
        }

        public async Task<List<StateDto>> GetAllStateAsync()
        {
            var ls = new List<StateDto>();
            var stateDict = await this._stateManager.GetOrAddAsync<IReliableDictionary2<long, StateDto>>(Consts.STATE_DICT);
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

        public async Task<StateDto> GetLatestJobStateDataAsync(long jobId)
        {
            var jobDict = await this._stateManager.GetOrAddAsync<IReliableDictionary2<long, JobDto>>(Consts.JOB_DICT);
            var stateDict = await this._stateManager.GetOrAddAsync<IReliableDictionary2<long, StateDto>>(Consts.STATE_DICT);
            using (var tx = this._stateManager.CreateTransaction())
            {
                var job_condition = await jobDict.TryGetValueAsync(tx, jobId);
                if (!job_condition.HasValue)
                    return null;
                var state_condition = await stateDict.TryGetValueAsync(tx, job_condition.Value.StateId);
                if (!state_condition.HasValue)
                    return null;
                return state_condition.Value;
            }
        }

        public async Task<List<StateDto>> GetStates(long jobId)
        {
            var stateDict = await this._stateManager.GetOrAddAsync<IReliableDictionary2<long, StateDto>>(Consts.STATE_DICT);
            using (var tx = this._stateManager.CreateTransaction())
            {
                var ls = new List<StateDto>();
                var enumlator = (await stateDict.CreateEnumerableAsync(tx)).GetAsyncEnumerator();
                while (await enumlator.MoveNextAsync(default))
                {
                    ls.Add(enumlator.Current.Value);
                }
                return ls;
            }
        }

        public async Task AddStateAsync(long jobId, StateDto state)
        {
            var stateDict = await this._stateManager.GetOrAddAsync<IReliableDictionary2<long, StateDto>>(Consts.STATE_DICT);
            using (var tx = this._stateManager.CreateTransaction())
            {
                var count = await stateDict.GetCountAsync(tx);
                state.Id = count + 1;
                state.JobId = jobId;
                await stateDict.SetAsync(tx, state.Id, state);
                await tx.CommitAsync();
            }
        }
    }
}
