using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire.States;
using HangFireStorageService.Dto;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;

namespace HangFireStorageService.Servces
{
    internal class JobStateProccesser
    {
        private readonly ITransaction _tx;
        private readonly IReliableDictionary2<long, StateDto> _state;
        private readonly IReliableDictionary2<long, JobDto> _job;


        public JobStateProccesser(ITransaction tx, IReliableDictionary2<long, StateDto> states, IReliableDictionary2<long, JobDto> jobs)
        {
            this._tx = tx;
            this._state = states;
            this._job = jobs;
        }

        public async Task Proccess_AddJobState(string jobId, IState state)
        {
            //TODO:should convert
            var stateCount = this._state.Count;
            await this._state.AddAsync(this._tx, ++stateCount, new StateDto());
            var job = await this._job.GetOrAddAsync(this._tx, long.Parse(jobId), new JobDto());
            //TODO:update job state point
            await this._job.TryUpdateAsync(this._tx);
        }
    }
}
