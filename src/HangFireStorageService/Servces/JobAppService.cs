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
    public class JobAppService : IJobAppService
    {
        private readonly IReliableStateManager _stateManager;

        public JobAppService(IReliableStateManager stateManager)
        {
            _stateManager = stateManager;
        }


        public async Task<JobDto> AddJobAsync(JobDto job)
        {
            if (job == null)
                throw new ArgumentNullException(nameof(job));
            var jobDict = await this._stateManager.GetOrAddAsync<IReliableDictionary2<long, JobDto>>(Consts.JOB_DICT);
            using (var tx = this._stateManager.CreateTransaction())
            {
                var count = await jobDict.GetCountAsync(tx);
                count++;
                job.Id = count;
                await jobDict.AddAsync(tx, job.Id, job);
                await tx.CommitAsync();
            }
            return job;
        }

        public async Task<JobDto> GetJobAsync(long JobId)
        {
            var jobDict = await this._stateManager.GetOrAddAsync<IReliableDictionary2<long, JobDto>>(Consts.JOB_DICT);
            using (var tx = this._stateManager.CreateTransaction())
            {
                var job_condition = await jobDict.TryGetValueAsync(tx, JobId);
                if (job_condition.HasValue)
                    return job_condition.Value;
                return null;
            }
        }

        public async Task UpdateJobAsync(JobDto job)
        {
            if (job == null)
                throw new ArgumentNullException(nameof(job));
            var jobDict = await this._stateManager.GetOrAddAsync<IReliableDictionary2<long, JobDto>>(Consts.JOB_DICT);
            using (var tx = this._stateManager.CreateTransaction())
            {
                await jobDict.SetAsync(tx, job.Id, job);
                await tx.CommitAsync();
            }
        }
    }
}
