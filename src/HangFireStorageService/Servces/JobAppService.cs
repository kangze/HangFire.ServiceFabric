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
    public class JobAppService : HangfireDataBaseService, IJobAppService
    {

        public JobAppService(IReliableStateManager stateManager, ServiceFabricOptions options, IMapper mapper)
            : base(stateManager, options, mapper)
        {

        }


        public async Task<JobDto> AddOrUpdateAsync(JobDto jobDto)
        {
            if (jobDto == null)
                throw new ArgumentNullException(nameof(jobDto));
            if (jobDto.Id == default)
                jobDto.Id = Guid.NewGuid().ToString();
            var job = this._mapper.Map<JobEntity>(jobDto);
            await this.InitDictAsync();
            using (var tx = this._stateManager.CreateTransaction())
            {
                await this._job_dict.SetAsync(tx, job.Id, job);
                await tx.CommitAsync();
                return jobDto;
            }
        }

        public async Task<List<JobDto>> GetJobsAsync(string JobId)
        {
            await this.InitDictAsync();
            using (var tx = this._stateManager.CreateTransaction())
            {
                var ls = new List<JobDto>();
                var emulator = (await this._job_dict.CreateEnumerableAsync(tx)).GetAsyncEnumerator();
                while (await emulator.MoveNextAsync(default))
                {
                    var jobDto = this._mapper.Map<JobDto>(emulator.Current.Value);
                    if (!string.IsNullOrEmpty(JobId) && jobDto.Id == JobId)
                    {
                        ls.Add(jobDto);
                        return ls;
                    }
                    else if (string.IsNullOrEmpty(JobId))
                    {
                        ls.Add(jobDto);
                    }
                }
                return ls;
            }
        }

        public async Task<List<JobDto>> GetJobsByStateNameAsync(string stateName)
        {
            await this.InitDictAsync();
            using (var tx = this._stateManager.CreateTransaction())
            {
                var ls = new List<JobDto>();
                var enumlator = (await this._job_dict.CreateEnumerableAsync(tx)).GetAsyncEnumerator();
                while (await enumlator.MoveNextAsync(default))
                {
                    if (enumlator.Current.Value.StateName == stateName)
                        ls.Add(this._mapper.Map<JobDto>(enumlator.Current.Value));
                }
                return ls;
            }
        }

        public async Task<List<JobDto>> GetJobDetailsAsync(string[] jobIds)
        {
            var result = new List<JobDto>();
            await this.InitDictAsync();
            using (var tx = this._stateManager.CreateTransaction())
            {
                foreach (var jobId in jobIds)
                {
                    var job_condition = await this._job_dict.TryGetValueAsync(tx, jobId);
                    if (job_condition.HasValue)
                    {
                        var jobDto = this._mapper.Map<JobDto>(job_condition.Value);
                        var state_condition = await this._state_dict.TryGetValueAsync(tx, job_condition.Value.StateId);
                        if (state_condition.HasValue)
                        {
                            jobDto.Reason = state_condition.Value.Reason;
                            jobDto.StateData = state_condition.Value.Data;
                            jobDto.StateChanged = state_condition.Value.CreatedAt;
                        }
                        result.Add(jobDto);
                    }
                    else
                    {
                        result.Add(new JobDto() { Id = jobId });
                    }
                }
            }
            return result;
        }

        public async Task AddJobStateAsync(string jobId, StateDto state)
        {
            await this.InitDictAsync();
            using (var tx = this._stateManager.CreateTransaction())
            {
                var job_condition = await this._job_dict.TryGetValueAsync(tx, jobId);
                if (!job_condition.HasValue)
                    return;
                state.Id = Guid.NewGuid().ToString();
                job_condition.Value.StateId = state.Id;
                job_condition.Value.StateName = state.Name;
                await this._state_dict.SetAsync(tx, state.Id, state);
                await this._job_dict.SetAsync(tx, jobId, job_condition.Value);
                await tx.CommitAsync();
            }
        }
    }
}
