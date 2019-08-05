using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.ServiceFabric.Internal;
using Hangfire.ServiceFabric.Servces;
using Hangfire.Storage;
using HangFireStorageService.Extensions;
using HangFireStorageService.Servces;

namespace HangFireStorageService.Internal
{
    public class ServiceFabricStorage : JobStorage
    {
        private readonly IServiceFabriceStorageServices _services;

        private ServiceFabricStorage(
            IServiceFabriceStorageServices servies
            )
        {
            this._services = servies;
        }

        internal static ServiceFabricStorage Create(ServiceFabricOptions option)
        {
            var services = RemotingClient.CreateServiceFabricStorageServices();
            return new ServiceFabricStorage(services);
        }


        public override IMonitoringApi GetMonitoringApi()
        {
            return new ServiceFabricMonitoringApi(this._services);
        }

        public override IStorageConnection GetConnection()
        {
            return new ServiceFabricStorageConnect(this._services);
        }
    }
}
