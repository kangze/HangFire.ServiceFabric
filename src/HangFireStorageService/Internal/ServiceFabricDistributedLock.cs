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

        public static List<string> LockString = new List<string>();

        private bool _completed;
        private static readonly object _lockObject = new object();

        public ServiceFabricDistributedLock(string resource, TimeSpan timeout, IResourceLockAppService resourceLockAppService)
        {
            this._resource = resource;
            AcquireLock();
        }

        public IDisposable AcquireLock()
        {
            lock (_lockObject)
            {
                while (LockString.Contains(this._resource))
                {
                    Thread.Sleep(1000);
                }
                LockString.Add(this._resource);
                return this;
            }

        }

        public void Dispose()
        {
            LockString.Remove(_resource);
            return;
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
