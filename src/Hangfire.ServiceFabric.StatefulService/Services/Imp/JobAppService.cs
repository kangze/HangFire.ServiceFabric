using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire.ServiceFabric.Extensions;
using Hangfire.ServiceFabric.Model;
using Hangfire.ServiceFabric.Model.Dtos;
using Hangfire.ServiceFabric.Model.Interfaces;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;

namespace Hangfire.ServiceFabric.StatefulService.Services.Imp
{
    public class JobAppService : IJobAppService
    {
        private readonly IReliableStateManager _stateManager;
        private readonly string _dictName;

        public JobAppService(IReliableStateManager stateManager, string dictName)
        {
            this._stateManager = stateManager;
            _dictName = dictName;
        }
        public async Task<JobDto> GetJobAsync(string jobId)
        {
            var jobDict = await StoreFactory.CreateJobDictAsync(this._stateManager, this._dictName);
            using (var tx = this._stateManager.CreateTransaction())
            {
                var jobCondition = await jobDict.TryGetValueAsync(tx, jobId);
                if (jobCondition.HasValue)
                    return jobCondition.Value;
                return null;
            }
        }

        public async Task<List<JobDto>> GetJobsByStateNameAsync(string stateName)
        {
            var jobDict = await this._stateManager.GetOrAddAsync<IReliableDictionary2<string, JobDto>>(Consts.JOB_DICT);
            using (var tx = this._stateManager.CreateTransaction())
            {
                var ls = new List<JobDto>();
                var enumlator = (await jobDict.CreateEnumerableAsync(tx)).GetAsyncEnumerator();
                while (await enumlator.MoveNextAsync(default))
                {
                    if (enumlator.Current.Value.StateName == stateName)
                        ls.Add(enumlator.Current.Value);
                }
                return ls;
            }
        }

        public async Task<List<JobDto>> GetJobsByIdsAsync(string[] jobIds)
        {
            var result = new List<JobDto>();
            var jobDict = await StoreFactory.CreateJobDictAsync(this._stateManager, this._dictName);
            using (var tx = this._stateManager.CreateTransaction())
            {
                foreach (var jobId in jobIds)
                {
                    var jobCondition = await jobDict.TryGetValueAsync(tx, jobId);
                    if (jobCondition.HasValue)
                        result.Add(jobCondition.Value);
                    else
                        result.Add(new JobDto() { Id = jobId });
                }
            }
            return result;
        }


        public async Task<List<JobDto>> GetJobsAsync()
        {
            var jobDict = await StoreFactory.CreateJobDictAsync(this._stateManager, this._dictName);
            using (var tx = this._stateManager.CreateTransaction())
            {
                var enumlator = (await jobDict.CreateEnumerableAsync(tx)).GetAsyncEnumerator();
                var ls = new List<JobDto>();
                while (await enumlator.MoveNextAsync(default))
                {
                    ls.Add(enumlator.Current.Value);
                }
                return ls;
            }
        }

        public async Task AddOrUpdateAsync(ITransaction tx, IReliableDictionary2<string, JobDto> jobDict, string key, Func<JobDto, JobDto> action)
        {
            var jobCondition = await jobDict.TryGetValueAsync(tx, key);
            if (!jobCondition.HasValue)
            {
                var job = action.Invoke(jobCondition.Value);
                await jobDict.AddOrUpdateAsync(tx, key, job, (k, v) => job);
            }
        }
    }
}
