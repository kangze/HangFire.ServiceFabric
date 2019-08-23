using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hangfire.ServiceFabric.StatefulService.Services.Imp;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Runtime;
using Hangfire.ServiceFabric.Model;

namespace Hangfire.ServiceFabric.StatefulService
{
    public abstract class HangfireStatefulService : Microsoft.ServiceFabric.Services.Runtime.StatefulService
    {
        private readonly string _prefix = "_default";

        protected HangfireStatefulService(StatefulServiceContext context, string prefix)
           : base(context)
        {
            if (!string.IsNullOrEmpty(prefix))
                this._prefix = prefix;
        }

        /// <summary>
        /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
        /// </summary>
        /// <remarks>
        /// For more information on service communication, see https://aka.ms/servicefabricservicecommunication
        /// </remarks>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return CreateServiceReplicaListeners(this.StateManager, this._prefix);
        }

        private static IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners(IReliableStateManager stateManager, string prefix)
        {
            var dictNames = new DictNames(prefix);

            return new[]
            {
                new ServiceReplicaListener((c) =>
                    new FabricTransportServiceRemotingListener(c, new JobAppService(stateManager,dictNames.JobDictName)) , Constants.ListenerNames_JobAppService),
                new ServiceReplicaListener((c) =>
                    new FabricTransportServiceRemotingListener(c, new JobQueueAppService(stateManager,dictNames.JobQueueDictName)) , Constants.ListenerNames_JobQueueAppService),
                new ServiceReplicaListener((c) =>
                    new FabricTransportServiceRemotingListener(c, new ServerAppService(stateManager,dictNames.ServerDictName)) , Constants.ListenerNames_ServerAppService),
                new ServiceReplicaListener((c) =>
                    new FabricTransportServiceRemotingListener(c, new CounterAppService(stateManager,dictNames.CounterDictName)) , Constants.ListenerNames_CounterAppService),
                new ServiceReplicaListener((c) =>
                    new FabricTransportServiceRemotingListener(c, new AggregatedCounterAppService(stateManager,dictNames.AggregatdcounterDictName)) , Constants.ListenerNames_AggregatedCounterAppService),
                new ServiceReplicaListener((c) =>
                    new FabricTransportServiceRemotingListener(c, new JobSetAppService(stateManager,dictNames.SetDictName)) , Constants.ListenerNames_JobSetAppService),
                new ServiceReplicaListener((c) =>
                    new FabricTransportServiceRemotingListener(c, new HashAppService(stateManager,dictNames.HashDictName)) , Constants.ListenerNames_HashAppService),
                new ServiceReplicaListener((c) =>
                    new FabricTransportServiceRemotingListener(c, new ResourceLockAppService(stateManager,dictNames.LockDictName)) , Constants.ListenerNames_ResourceLockAppService),
                new ServiceReplicaListener((c) =>
                    new FabricTransportServiceRemotingListener(c, new TransactionAppService(stateManager,dictNames)) , Constants.ListenerNames_TransactionAppService)
            };

        }
    }
}
