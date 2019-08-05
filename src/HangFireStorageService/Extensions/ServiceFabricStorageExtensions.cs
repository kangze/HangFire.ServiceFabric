﻿using Hangfire;
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
            [NotNull] this IGlobalConfiguration configuration)
        {
            var storage = ServiceFabricStorage.Create();
            return configuration.UseStorage(storage);
        }


        public static IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners(IReliableStateManager stateManager)
        {
            return new[]
            {
                new ServiceReplicaListener((c) =>
                     new FabricTransportServiceRemotingListener(c, new JobAppService(stateManager)) , Constants.ListenerNames_JobAppService),
                new ServiceReplicaListener((c) =>
                     new FabricTransportServiceRemotingListener(c, new JobQueueuAppService(stateManager)) , Constants.ListenerNames_JobQueueAppService),
                new ServiceReplicaListener((c) =>
                     new FabricTransportServiceRemotingListener(c, new ServerAppService(stateManager)) , Constants.ListenerNames_ServerAppService),
                new ServiceReplicaListener((c) =>
                     new FabricTransportServiceRemotingListener(c, new CounterAppService(stateManager)) , Constants.ListenerNames_CounterAppService),
                new ServiceReplicaListener((c) =>
                     new FabricTransportServiceRemotingListener(c, new AggregatedCounterAppService(stateManager)) , Constants.ListenerNames_AggregatedCounterAppService),
                new ServiceReplicaListener((c) =>
                     new FabricTransportServiceRemotingListener(c, new JobSetAppService(stateManager)) , Constants.ListenerNames_JobSetAppService),
                new ServiceReplicaListener((c) =>
                     new FabricTransportServiceRemotingListener(c, new HashAppService(stateManager)) , Constants.ListenerNames_HashAppService),
                new ServiceReplicaListener((c) =>
                     new FabricTransportServiceRemotingListener(c, new ResourceLockAppService(stateManager)) , Constants.ListenerNames_ResourceLockAppService)
            };
        }
    }
}
