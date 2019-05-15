using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire.States;
using HangFireStorageService.Dto;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Newtonsoft.Json;

namespace HangFireStorageService.Servces
{
    internal class JobAndStateProccesser
    {
        private readonly ITransaction _tx;
        private readonly IReliableDictionary2<long, List<StateDto>> _state;
        private readonly IReliableDictionary2<long, JobDto> _job;


        public JobAndStateProccesser(ITransaction tx, IReliableDictionary2<long, List<StateDto>> states, IReliableDictionary2<long, JobDto> jobs)
        {
            this._tx = tx;
            this._state = states;
            this._job = jobs;
        }

        public async Task<long> Proccess_AddJobState(string jobId, IState state)
        {
            var data = state.SerializeData();
            var id = long.Parse(jobId);
            var stateCount = this._state.Count;
            var stateDto = new StateDto()
            {
                Id = ++stateCount,
                JobId = id,
                CreatedAt = DateTime.UtcNow,
                Name = state.Name,
                Data = data == null ? null : JsonConvert.SerializeObject(data),
                Reason = state.Reason,
            };
            var states = await this._state.GetOrAddAsync(this._tx, id, new List<StateDto>());
            states.Add(stateDto);
            await this._state.AddOrUpdateAsync(this._tx, id, states, (k, v) => states);
            return stateCount;
        }

        public async Task Proccess_SetJobState(string jobId, IState state)
        {
            var id = long.Parse(jobId);
            var stateId = await this.Proccess_AddJobState(jobId, state);
            var jobDto = await this.GetJobAsync(id);
            jobDto.StateId = stateId;
            jobDto.StateName = state.Name;
            await this._job.SetAsync(this._tx, id, jobDto);
        }

        public async Task Prccess_ExpireJob(string jobId, TimeSpan expireIn)
        {
            var id = long.Parse(jobId);
            var job = await this.GetJobAsync(id);
            job.ExpireAt = DateTime.UtcNow.Add(expireIn);
            await this._job.SetAsync(this._tx, id, job);
        }

        public async Task Proccess_PersistJob(string jobId)
        {
            var id = long.Parse(jobId);
            var job = await this.GetJobAsync(id);
            job.ExpireAt = null;
            await this._job.SetAsync(this._tx, id, job);
        }

        private async Task<JobDto> GetJobAsync(long id)
        {
            var jobDto_contional = await this._job.TryGetValueAsync(this._tx, id);
            if (!jobDto_contional.HasValue)
                throw new Exception("Not Found Job,JobId:" + id);
            return jobDto_contional.Value;
        }

    }
}
