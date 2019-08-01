using AutoMapper;
using Hangfire.ServiceFabric.Dtos;
using Hangfire.ServiceFabric.Entities;
using HangFireStorageService;
using HangFireStorageService.Dto;
using HangFireStorageService.Extensions;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.ServiceFabric.Servces
{
    public abstract class HangfireDataBaseService
    {
        protected readonly IReliableStateManager _stateManager;
        protected readonly ServiceFabricOptions _options;


        protected IReliableDictionary2<string, JobDto> _job_dict;
        protected IReliableDictionary2<string, StateEntity> _state_dict;


        public HangfireDataBaseService(IReliableStateManager stateManager, ServiceFabricOptions options)
        {
            this._stateManager = stateManager;
            this._options = options;
        }

        protected async Task InitDictAsync()
        {
            this._job_dict = await this._stateManager.GetOrAddAsync<IReliableDictionary2<string, JobDto>>(string.Format(Consts.JOB_DICT, this._options.Prefix));
            this._state_dict = await this._stateManager.GetOrAddAsync<IReliableDictionary2<string, StateEntity>>(string.Format(Consts.STATE_DICT, this._options.Prefix));
        }
    }
}
