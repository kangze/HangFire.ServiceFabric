using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hangfire.ServiceFabric.Internal
{
    public class ServiceFabricDistributedLock : IDisposable
    {
        private readonly string _resource;
        private readonly Guid _lockId;

        private readonly static Dictionary<string, HashSet<Guid>> _lockedResources = new Dictionary<string, HashSet<Guid>>();

        public ServiceFabricDistributedLock(string resource, Guid lockId)
        {
            _resource = resource;
            _lockId = lockId;
        }

        public static IDisposable AcquireLock(string resource, TimeSpan timeout)
        {
            var lockId = Guid.NewGuid();

            if (!_lockedResources.ContainsKey(resource))
            {
                //Lock Resource By ServiceFabric
                //SqlServerDistributedLock.Acquire(_dedicatedConnection, resource, timeout);
                _lockedResources.Add(resource, new HashSet<Guid>());
            }

            _lockedResources[resource].Add(lockId);
            return new ServiceFabricDistributedLock(resource, lockId);
        }

        public void Dispose()
        {
            if (_lockedResources.ContainsKey(_resource) &&
                _lockedResources[_resource].Contains(_lockId) &&
                _lockedResources[_resource].Remove(_lockId) &&
                _lockedResources[_resource].Count == 0 &&
                _lockedResources.Remove(_resource)
                )
            {
                //TODO:ServiceFabric Reliease It

            }
        }
    }
}
