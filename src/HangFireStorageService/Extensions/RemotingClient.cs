using HangFireStorageService.Servces;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Client;
using ServiceFabricContrib;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HangFireStorageService.Extensions
{
    /// <summary>
    /// Remoting,暂时不考虑分区
    /// </summary>
    internal static class RemotingClient
    {
        private static ServiceProxyFactory proxyFactory = new ServiceProxyFactory((c) =>
           new FabricTransportServiceRemotingClientFactory());
        private static FabricClient fabricClient = new FabricClient();

        public static string ApplicationUri { get; set; }

        public static IJobAppService CreateJobAppService()
        {
            var uri = new Uri(ApplicationUri);
            return proxyFactory.CreateServiceProxy<IJobAppService>(uri, listenerName: Constants.ListenerNames_JobAppService);
        }

        public static IJobQueueAppService CreateJobQueueAppService()
        {
            var uri = new Uri(ApplicationUri);
            var services = proxyFactory.CreateServiceProxy<IJobQueueAppService>(uri, listenerName: Constants.ListenerNames_JobQueueAppService);
            return services;
        }


        public static IServerAppService CreateServiceAppService()
        {
            var uri = new Uri(ApplicationUri);
            return proxyFactory.CreateServiceProxy<IServerAppService>(uri, listenerName: Constants.ListenerNames_ServerAppService);
        }

        public static IAggregatedCounterAppService CreateAggregateCounterAppService()
        {
            var uri = new Uri(ApplicationUri);
            return proxyFactory.CreateServiceProxy<IAggregatedCounterAppService>(uri, listenerName: Constants.ListenerNames_AggregatedCounterAppService);
        }

        public static ICounterAppService CreateCounterAppService()
        {
            var uri = new Uri(ApplicationUri);
            return proxyFactory.CreateServiceProxy<ICounterAppService>(uri, listenerName: Constants.ListenerNames_CounterAppService);
        }

        public static IJobSetAppService CreateJobSetAppService()
        {
            var uri = new Uri(ApplicationUri);
            return proxyFactory.CreateServiceProxy<IJobSetAppService>(uri, listenerName: Constants.ListenerNames_JobSetAppService);
        }

        public static IHashAppService CreateHashAppService()
        {
            var uri = new Uri(ApplicationUri);
            return proxyFactory.CreateServiceProxy<IHashAppService>(uri, listenerName: Constants.ListenerNames_HashAppService);
        }

        public static IListAppService CreateJobListAppService()
        {
            var uri = new Uri(ApplicationUri);
            return proxyFactory.CreateServiceProxy<IListAppService>(uri, listenerName: Constants.ListenerNames_jobListAppSerivce);
        }
    }
}
