using Hangfire.ServiceFabric.Servces;
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
        private readonly IResourceLockAppService _resourceLockAppService;

        private readonly static Dictionary<string, HashSet<Guid>> _lockedResources = new Dictionary<string, HashSet<Guid>>();

        public ServiceFabricDistributedLock(string resource, IResourceLockAppService resourceLockAppService)
        {
            _resource = resource;
            _lockId = Guid.NewGuid();
            _resourceLockAppService = resourceLockAppService;
        }

        public IDisposable AcquireLock()
        {
            var lockId = Guid.NewGuid();

            if (!_lockedResources.ContainsKey(_resource))
            {

                //SqlServerDistributedLock.Acquire(_dedicatedConnection, resource, timeout);
                this._resourceLockAppService.LockAsync(this._resource).GetAwaiter().GetResult();
                _lockedResources.Add(_resource, new HashSet<Guid>());
            }

            _lockedResources[_resource].Add(lockId);
            return this;
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
