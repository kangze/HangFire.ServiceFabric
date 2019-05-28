using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Storage;

namespace HangFireStorageService.Internal
{
    public class ServiceFabricStorage : JobStorage
    {
        

        public override IMonitoringApi GetMonitoringApi()
        {
            return new ServiceFabricMonitoringApi();
        }

        public override IStorageConnection GetConnection()
        {
            return new ServiceFabricStorageConnect();
        }
    }
}
