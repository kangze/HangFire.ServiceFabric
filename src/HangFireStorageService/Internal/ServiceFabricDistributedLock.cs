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
        private readonly IResourceLockAppService _resourceLockAppService;
        private static readonly SemaphoreSlim SemaphoreSlim = new SemaphoreSlim(1);
        private static readonly ThreadLocal<Dictionary<string, int>> AcquiredLocks
                 = new ThreadLocal<Dictionary<string, int>>(() => new Dictionary<string, int>());

        private bool _completed;
        private readonly object _lockObject = new object();

        public ServiceFabricDistributedLock(string resource, TimeSpan timeout, IResourceLockAppService resourceLockAppService)
        {
            if (string.IsNullOrEmpty(resource))
                throw new ArgumentException(nameof(resource));
            _resource = resource;
            _resourceLockAppService = resourceLockAppService;

            if (!AcquiredLocks.Value.ContainsKey(_resource) || AcquiredLocks.Value[_resource] == 0)
            {

                AcquireLock(timeout);
                AcquiredLocks.Value[_resource] = 1;
            }
            else
            {
                AcquiredLocks.Value[_resource]++;
            }
        }

        public IDisposable AcquireLock(TimeSpan timeout)
        {
            var now = DateTime.UtcNow;
            var lockTimeoutTime = now.Add(timeout);
            var isLockAcquired = false;
            while (!isLockAcquired && lockTimeoutTime >= now)
            {
                //success gain a lock
                if (!this._resourceLockAppService.LockAsync(this._resource).GetAwaiter().GetResult())
                {
                    isLockAcquired = true;
                    continue;
                }

                //wait for mutex if can not gain a lock
                SemaphoreSlim.Wait();
            }
            if (!isLockAcquired)
            {
                throw new Exception($"{_resource} - Can not Gain Lock,Because of Timeout !");
            }
            return this;
        }

        public void Dispose()
        {
            if (_completed)
                return;
            _completed = true;
            if (!AcquiredLocks.Value.ContainsKey(_resource))
                return;


            AcquiredLocks.Value[_resource]--;
            if (AcquiredLocks.Value[_resource] > 0)
                return;


            // Timer callback may be invoked after the Dispose method call,
            // so we are using lock to avoid un synchronized calls.
            lock (_lockObject)
            {
                AcquiredLocks.Value.Remove(_resource);
                this._resourceLockAppService.ReleaseAsync(this._resource).GetAwaiter().GetResult();
            }
        }
    }
}
