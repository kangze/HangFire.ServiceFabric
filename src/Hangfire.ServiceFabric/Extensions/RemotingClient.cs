using ServiceFabricContrib;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire.ServiceFabric.Internal;
using Hangfire.ServiceFabric.Model;
using Hangfire.ServiceFabric.Model.Interfaces;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Client;

namespace Hangfire.ServiceFabric.Extensions
{
    /// <summary>
    /// Remoting,暂时不考虑分区
    /// </summary>
    internal class RemotingClient
    {
        private static readonly ServiceProxyFactory ProxyFactory = new ServiceProxyFactory((c) =>
           new FabricTransportServiceRemotingClientFactory());

        private readonly Uri _applicationUri;
        private readonly string _partitionKey;

        public RemotingClient(string applicationUri, string partitionKey)
        {
            this._applicationUri = new Uri(applicationUri);
            this._partitionKey = partitionKey;
        }




        public IJobAppService CreateJobAppService()
        {
            return ProxyFactory.CreateServiceProxy<IJobAppService>(_applicationUri,new ServicePartitionKey(this._partitionKey), listenerName: Constants.ListenerNames_JobAppService);
        }

        public IJobQueueAppService CreateJobQueueAppService()
        {
            var services = ProxyFactory.CreateServiceProxy<IJobQueueAppService>(_applicationUri, new ServicePartitionKey(this._partitionKey), listenerName: Constants.ListenerNames_JobQueueAppService);
            return services;
        }


        public IServerAppService CreateServiceAppService()
        {
            return ProxyFactory.CreateServiceProxy<IServerAppService>(_applicationUri, new ServicePartitionKey(this._partitionKey), listenerName: Constants.ListenerNames_ServerAppService);
        }

        public IAggregatedCounterAppService CreateAggregateCounterAppService()
        {
            return ProxyFactory.CreateServiceProxy<IAggregatedCounterAppService>(_applicationUri, new ServicePartitionKey(this._partitionKey), listenerName: Constants.ListenerNames_AggregatedCounterAppService);
        }

        public ICounterAppService CreateCounterAppService()
        {
            return ProxyFactory.CreateServiceProxy<ICounterAppService>(_applicationUri, new ServicePartitionKey(this._partitionKey), listenerName: Constants.ListenerNames_CounterAppService);
        }

        public IJobSetAppService CreateJobSetAppService()
        {
            return ProxyFactory.CreateServiceProxy<IJobSetAppService>(_applicationUri, new ServicePartitionKey(this._partitionKey), listenerName: Constants.ListenerNames_JobSetAppService);
        }

        public IHashAppService CreateHashAppService()
        {
            return ProxyFactory.CreateServiceProxy<IHashAppService>(_applicationUri, new ServicePartitionKey(this._partitionKey), listenerName: Constants.ListenerNames_HashAppService);
        }

        public IListAppService CreateJobListAppService()
        {
            return ProxyFactory.CreateServiceProxy<IListAppService>(_applicationUri, new ServicePartitionKey(this._partitionKey), listenerName: Constants.ListenerNames_jobListAppSerivce);
        }
        public IResourceLockAppService CreateResourceLockAppService()
        {
            return ProxyFactory.CreateServiceProxy<IResourceLockAppService>(_applicationUri, new ServicePartitionKey(this._partitionKey), listenerName: Constants.ListenerNames_ResourceLockAppService);
        }

        public ITransactionAppService CreateTransactionAppService()
        {
            return ProxyFactory.CreateServiceProxy<ITransactionAppService>(_applicationUri, new ServicePartitionKey(this._partitionKey), listenerName: Constants.ListenerNames_TransactionAppService);
        }

        public IServiceFabriceStorageServices CreateServiceFabricStorageServices()
        {
            var service = new ServiceFabricStorageServices(
                CreateJobQueueAppService(),
                CreateJobAppService(),
                CreateServiceAppService(),
                CreateCounterAppService(),
                CreateAggregateCounterAppService(),
                CreateJobSetAppService(),
                CreateHashAppService(),
                CreateJobListAppService(),
                CreateResourceLockAppService(),
                CreateTransactionAppService()
                );
            return service;
        }
    }
}
