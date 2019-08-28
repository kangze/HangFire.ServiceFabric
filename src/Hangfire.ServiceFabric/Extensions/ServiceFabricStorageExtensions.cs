using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Annotations;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Runtime;
using Hangfire.ServiceFabric.Internal;

namespace Hangfire.ServiceFabric.Extensions
{
    public static class ServiceFabricStorageExtensions
    {
        public static IGlobalConfiguration<ServiceFabricStorage> UseServiceFabric(
            [NotNull] this IGlobalConfiguration configuration,
            [NotNull] string applicationUri,
            [NotNull] string partitionKey)
        {
            return UseServiceFabric(configuration, applicationUri, partitionKey, new ServiceFabricStorageOption());
        }

        public static IGlobalConfiguration<ServiceFabricStorage> UseServiceFabric(
            [NotNull] this IGlobalConfiguration configuration,
            [NotNull] string applicationUri,
            [NotNull] string partitionKey,
            [NotNull] ServiceFabricStorageOption option)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            if (string.IsNullOrEmpty(applicationUri)) throw new ArgumentNullException(nameof(applicationUri));
            if (option == null) throw new ArgumentNullException(nameof(option));

            var remotingClient = new RemotingClient(applicationUri, partitionKey);
            var storage = new ServiceFabricStorage(remotingClient.CreateServiceFabricStorageServices());
            return configuration.UseStorage(storage);
        }
    }
}
