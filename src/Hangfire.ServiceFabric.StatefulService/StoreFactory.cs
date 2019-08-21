using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire.ServiceFabric.Model.Dtos;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;

namespace Hangfire.ServiceFabric.StatefulService
{
    public static class StoreFactory
    {
        public static async Task<IReliableDictionary2<string, JobDto>> CreateJobDictAsync(IReliableStateManager stateManager, string dictName)
        {
            return await stateManager.GetOrAddAsync<IReliableDictionary2<string, JobDto>>(dictName);
        }
    }
}
