using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public JobAppService(IReliableStateManager stateManager, ServiceFabricOptions options)
            : base(stateManager, options)
        {

        }


        public async Task<JobDto> AddOrUpdateAsync(JobDto jobDto)
        {
            if (jobDto == null)
                throw new ArgumentNullException(nameof(jobDto));
            if (jobDto.Id == default)
                jobDto.Id = Guid.NewGuid().ToString("N");
            await this.InitDictAsync();
            using (var tx = this._stateManager.CreateTransaction())
            {
                await this._job_dict.SetAsync(tx, jobDto.Id, jobDto);
                await tx.CommitAsync();
                return jobDto;
            }
        }

        public async Task<JobDto> GetJobAsync(string JobId)
        {
            await this.InitDictAsync();
            using (var tx = this._stateManager.CreateTransaction())
            {
                var job_condition = await this._job_dict.TryGetValueAsync(tx, JobId);
                if (job_condition.HasValue)
                    return job_condition.Value;
                return null;
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
                        ls.Add(enumlator.Current.Value);
                }
                return ls;
            }
        }

        public async Task<List<JobDto>> GetJobsByIdsAsync(string[] jobIds)
        {
            var result = new List<JobDto>();
            await this.InitDictAsync();
            using (var tx = this._stateManager.CreateTransaction())
            {
                foreach (var jobId in jobIds)
                {
                    var job_condition = await this._job_dict.TryGetValueAsync(tx, jobId);
                    if (job_condition.HasValue)
                        result.Add(job_condition.Value);
                    else
                        result.Add(new JobDto() { Id = jobId });
                }
            }
            return result;
        }


        public async Task<List<JobDto>> GetJobsAsync()
        {
            await this.InitDictAsync();
            using (var tx = this._stateManager.CreateTransaction())
            {
                var enumlator = (await this._job_dict.CreateEnumerableAsync(tx)).GetAsyncEnumerator();
                var ls = new List<JobDto>();
                while (await enumlator.MoveNextAsync(default))
                {
                    ls.Add(enumlator.Current.Value);
                }
                return ls;
            }
        }
    }
}
