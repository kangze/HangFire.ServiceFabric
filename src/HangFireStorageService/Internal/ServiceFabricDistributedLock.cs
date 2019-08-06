using Hangfire.ServiceFabric.Servces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private readonly TimeSpan _timeout;
        private readonly IResourceLockAppService _resourceLockAppService;

        private readonly static Dictionary<string, HashSet<Guid>> _lockedResources = new Dictionary<string, HashSet<Guid>>();

        public ServiceFabricDistributedLock(string resource, TimeSpan timeout, IResourceLockAppService resourceLockAppService)
        {
            _resource = resource;
            _lockId = Guid.NewGuid();
            _timeout = timeout;
            _resourceLockAppService = resourceLockAppService;
        }
        public static bool locked = false;

        public IDisposable AcquireLock()
        {
            while (!locked)
            {
                Thread.Sleep(100);
                locked = true;
            }
            return this;
        }

        public void Dispose()
        {
            locked = false;
            if (_lockedResources.ContainsKey(_resource) &&
                _lockedResources[_resource].Contains(_lockId) &&
                _lockedResources[_resource].Remove(_lockId) &&
                _lockedResources[_resource].Count == 0 &&
                _lockedResources.Remove(_resource)
                )
            {
                //TODO:ServiceFabric Reliease It
                this._resourceLockAppService.ReleaseAsync(_resource).GetAwaiter().GetResult();
                return;
            }
        }
    }
}
