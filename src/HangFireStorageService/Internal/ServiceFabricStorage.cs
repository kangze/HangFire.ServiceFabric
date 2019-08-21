using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.ServiceFabric.Internal;
using Hangfire.ServiceFabric.Model;
using Hangfire.Storage;

namespace Hangfire.ServiceFabric.Internal
{
    public class ServiceFabricStorage : JobStorage
    {
        private readonly IServiceFabriceStorageServices _services;

        public ServiceFabricStorage(IServiceFabriceStorageServices services)
        {
            this._services = services;
        }


        public override IMonitoringApi GetMonitoringApi()
        {
            return new ServiceFabricMonitoringApi(this._services);
        }

        public override IStorageConnection GetConnection()
        {
            return new ServiceFabricStorageConnection(this._services);
        }
    }
}
