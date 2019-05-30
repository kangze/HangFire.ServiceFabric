using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HangFireStorageService.Dto;
using HangFireStorageService.Extensions;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;

namespace HangFireStorageService.Servces
{
    public class JobAppService : IJobAppService
    {
        private readonly IReliableStateManager _stateManager;
        private readonly ServiceFabricOptions _options;

        public JobAppService(IReliableStateManager stateManager, ServiceFabricOptions options)
        {
            this._stateManager = stateManager;
            this._options = options;
        }


        public async Task<JobDto> AddJobAsync(JobDto job)
        {
            if (job == null)
                throw new ArgumentNullException(nameof(job));
            var jobDict = await this._stateManager.GetOrAddAsync<IReliableDictionary2<long, JobDto>>(string.Format(Consts.JOB_DICT, this._options.Prefix));
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

        public async Task<List<JobDto>> GetAllJobsAsync()
        {
            var ls = new List<JobDto>();
            var jobDict = await this._stateManager.GetOrAddAsync<IReliableDictionary2<long, JobDto>>(string.Format(Consts.JOB_DICT, this._options.Prefix));
            using (var tx = this._stateManager.CreateTransaction())
            {
                var emulator = (await jobDict.CreateEnumerableAsync(tx)).GetAsyncEnumerator();
                while (await emulator.MoveNextAsync(default))
                {
                    ls.Add(emulator.Current.Value);
                }
                return ls;
            }
        }

        public async Task<int> GetNumberbyStateName(string stateName)
        {
            var jobDict = await this._stateManager.GetOrAddAsync<IReliableDictionary2<long, JobDto>>(string.Format(Consts.JOB_DICT, this._options.Prefix));
            using (var tx = this._stateManager.CreateTransaction())
            {
                var count = 0;
                var enumlator = (await jobDict.CreateEnumerableAsync(tx)).GetAsyncEnumerator();
                while (await enumlator.MoveNextAsync(default))
                {
                    if (enumlator.Current.Value.StateName == stateName)
                        ++count;
                }
                return count;
            }
        }

        public async Task<JobDto> GetJobAsync(long JobId)
        {
            var jobDict = await this._stateManager.GetOrAddAsync<IReliableDictionary2<long, JobDto>>(string.Format(Consts.JOB_DICT, this._options.Prefix));
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

        public async Task<List<JobDetail>> GetJobDetailsAsync(long[] jobIds)
        {
            var job_dict = await this._stateManager.GetOrAddAsync<IReliableDictionary2<long, JobDto>>(string.Format(Consts.JOB_DICT, this._options.Prefix));
            var state_dict = await this._stateManager.GetOrAddAsync<IReliableDictionary2<long, StateDto>>(string.Format(Consts.STATE_DICT, this._options.Prefix));
            var result = new List<JobDetail>();
            using (var tx = this._stateManager.CreateTransaction())
            {
                foreach (var jobId in jobIds)
                {
                    var job_condition = await job_dict.TryGetValueAsync(tx, jobId);
                    if (job_condition.HasValue)
                    {
                        var jobDetail = new JobDetail()
                        {
                            Id = job_condition.Value.Id,
                            InvocationData = job_condition.Value.InvocationData,
                            Arguments = job_condition.Value.Arguments,
                            CreateAt = job_condition.Value.CreatedAt,
                            ExpireAt = job_condition.Value.ExpireAt,
                            StateName = job_condition.Value.StateName,
                            StateId = job_condition.Value.StateId
                        };
                        var state_condition = await state_dict.TryGetValueAsync(tx, job_condition.Value.StateId);
                        if (state_condition.HasValue)
                        {
                            jobDetail.Reason = state_condition.Value.Reason;
                            jobDetail.StateData = state_condition.Value.Data;
                            jobDetail.StateChanged = state_condition.Value.CreatedAt;
                        }
                        result.Add(jobDetail);
                    }
                    else
                    {
                        result.Add(new JobDetail() { Id = jobId });
                    }
                }
            }
            return result;
        }

        public async Task SetJobStateAsync(long jobId, StateDto state)
        {
            var stateDict = await this._stateManager.GetOrAddAsync<IReliableDictionary2<long, StateDto>>(Consts.STATE_DICT);
            var job_dict = await this._stateManager.GetOrAddAsync<IReliableDictionary2<long, JobDto>>(string.Format(Consts.JOB_DICT, this._options.Prefix));
            using (var tx = this._stateManager.CreateTransaction())
            {
                var job_condition = await job_dict.TryGetValueAsync(tx, jobId);
                if (!job_condition.HasValue)
                    return;
                var count = stateDict.Count;
                state.Id = count + 1;
                job_condition.Value.StateId = state.Id;
                job_condition.Value.StateName = state.Name;
                await stateDict.SetAsync(tx, state.Id, state);
                await job_dict.SetAsync(tx, jobId, job_condition.Value);
                await tx.CommitAsync();
            }
        }
    }
}
