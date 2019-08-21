using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hangfire.ServiceFabric.Model.Interfaces;

namespace Hangfire.ServiceFabric.Internal
{
    public class ServiceFabricDistributedLock : IDisposable
    {
        private readonly string _resource;
        private readonly TimeSpan _timeout;
        private static readonly SemaphoreSlim Slim = new SemaphoreSlim(1, 1);
        private readonly IResourceLockAppService _resourceLockAppService;
        private readonly static Dictionary<string, HashSet<Guid>> LockedResources = new Dictionary<string, HashSet<Guid>>();
        private Guid LockId { get; set; }


        public ServiceFabricDistributedLock(string resource, TimeSpan timeout, IResourceLockAppService resourceLockAppService)
        {
            this._resource = resource;
            this._timeout = timeout;
            this._resourceLockAppService = resourceLockAppService;
            AcquireLock();
        }

        public IDisposable AcquireLock()
        {

            Slim.Wait();
            var lockId = Guid.NewGuid();
            if (!LockedResources.ContainsKey(_resource))
            {
                try
                {
                    var started = Stopwatch.StartNew();
                    do
                    {
                        var b = _resourceLockAppService.LockAsync(_resource).GetAwaiter().GetResult();
                        if (b) break;
                        Thread.Sleep(350);
                    }
                    while (started.Elapsed < _timeout);
                }
                catch (Exception)
                {

                    Slim.Release();
                    throw;
                }

                LockedResources.Add(_resource, new HashSet<Guid>());
            }

            LockedResources[_resource].Add(lockId);
            this.LockId = lockId;
            Slim.Release();
            return this;
        }

        public void Dispose()
        {
            if (LockedResources.ContainsKey(_resource))
            {
                if (LockedResources[_resource].Contains(LockId))
                {
                    if (LockedResources[_resource].Remove(LockId) && LockedResources[_resource].Count == 0 && LockedResources.Remove(_resource))
                    {
                        this._resourceLockAppService.ReleaseAsync(_resource).GetAwaiter().GetResult();
                    }
                }
            }
        }
    }
}
