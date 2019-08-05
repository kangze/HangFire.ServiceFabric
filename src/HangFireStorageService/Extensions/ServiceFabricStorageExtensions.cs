using Hangfire;
using Hangfire.Annotations;
using Hangfire.ServiceFabric.Servces;
using HangFireStorageService.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HangFireStorageService.Extensions
{
    public static class ServiceFabricStorageExtensions
    {
        public static IGlobalConfiguration<ServiceFabricStorage> UseServiceFabric(
            [NotNull] this IGlobalConfiguration configuration,
            [NotNull] ServiceFabricOptions options)
        {
            if (options == null)
                throw new ArgumentException(nameof(options));
            var storage = ServiceFabricStorage.Create(options);
            return configuration.UseStorage(storage);
        }


        public static IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners(IReliableStateManager stateManager, ServiceFabricOptions options)
        {
            return new[]
            {
                new ServiceReplicaListener((c) =>
                     new FabricTransportServiceRemotingListener(c, new JobAppService(stateManager,options)) , Constants.ListenerNames_JobAppService),
                new ServiceReplicaListener((c) =>
                     new FabricTransportServiceRemotingListener(c, new JobQueueuAppService(stateManager,options)) , Constants.ListenerNames_JobQueueAppService),
                new ServiceReplicaListener((c) =>
                     new FabricTransportServiceRemotingListener(c, new ServerAppService(stateManager,options)) , Constants.ListenerNames_ServerAppService),
                new ServiceReplicaListener((c) =>
                     new FabricTransportServiceRemotingListener(c, new CounterAppService(stateManager,options)) , Constants.ListenerNames_CounterAppService),
                new ServiceReplicaListener((c) =>
                     new FabricTransportServiceRemotingListener(c, new AggregatedCounterAppService(stateManager,options)) , Constants.ListenerNames_AggregatedCounterAppService),
                new ServiceReplicaListener((c) =>
                     new FabricTransportServiceRemotingListener(c, new JobSetAppService(stateManager,options)) , Constants.ListenerNames_JobSetAppService),
                new ServiceReplicaListener((c) =>
                     new FabricTransportServiceRemotingListener(c, new HashAppService(stateManager,options)) , Constants.ListenerNames_HashAppService),
                new ServiceReplicaListener((c) =>
                     new FabricTransportServiceRemotingListener(c, new ResourceLockAppService(stateManager,options)) , Constants.ListenerNames_ResourceLockAppService)
            };
        }
    }
}
