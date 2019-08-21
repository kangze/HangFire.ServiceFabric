using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire.ServiceFabric.Model.Dtos;
using Hangfire.ServiceFabric.Model.Interfaces;
using Hangfire.ServiceFabric.Model.TransactionOperations;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;

namespace Hangfire.ServiceFabric.StatefulService.Services.Imp
{
    public class TransactionAppService : ITransactionAppService
    {
        private readonly IReliableStateManager _stateManager;
        private readonly string _dictName;

        private IReliableDictionary2<string, JobDto> _jobDtoDict;

        public TransactionAppService(IReliableStateManager stateManager, string dictName)
        {
            _stateManager = stateManager;
            _dictName = dictName;
        }


        public async Task CommitAsync(List<Operation> operations)
        {
            await InitDataStoresAsync();

            //impl Transaction
            using (var tx = this._stateManager.CreateTransaction())
            {

            }

        }

        private async Task InitDataStoresAsync()
        {
            this._jobDtoDict = await StoreFactory.CreateJobDictAsync(this._stateManager, this._dictName);
        }
    }
}
