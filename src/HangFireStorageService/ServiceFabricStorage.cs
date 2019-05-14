using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Storage;

namespace HangFireStorageService
{
    public class ServiceFabricStorage: JobStorage
    {
        public override IMonitoringApi GetMonitoringApi()
        {
            throw new NotImplementedException();
        }

        public override IStorageConnection GetConnection()
        {
            throw new NotImplementedException();
        }
    }
}
