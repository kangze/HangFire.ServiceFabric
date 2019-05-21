using Mcs.Common.BaseServices;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HangFireStorageService.Internal
{
    class ServicesFabricDistributedLock : IDisposable
    {
        private readonly ISimpleKeyValueService _simpleKeyValueService;
        private readonly Dictionary<string, HashSet<Guid>> _lockedResources = new Dictionary<string, HashSet<Guid>>();

        private readonly string _resourceName;
        private readonly TimeSpan _timeOut;

        public ServicesFabricDistributedLock(ISimpleKeyValueService simpleKeyValueService, string resource, TimeSpan timeout)
        {
            this._simpleKeyValueService = simpleKeyValueService;
            this._resourceName = resource;
            this._timeOut = timeout;
        }

        public void Dispose()
        {
            this.ReleaseLock(new Guid()).GetAwaiter().GetResult();
        }

        public async Task ReleaseLock(Guid lockId)
        {
            try
            {
                //本地有这个记录
                if (this._lockedResources.ContainsKey(this._resourceName))
                {
                    if (this._lockedResources[this._resourceName].Contains(lockId))
                    {
                        //Remove it form lock
                        if (this._lockedResources[this._resourceName].Remove(lockId) &&
                            this._lockedResources[this._resourceName].Count == 0 &&
                            this._lockedResources.Remove(this._resourceName))
                        {
                            //Remove it from Service Fabric
                            await this._simpleKeyValueService.Remove("", this._resourceName);
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IDisposable> AcquireLock()
        {
            var lockId = Guid.NewGuid();
            if (!this._lockedResources.ContainsKey(this._resourceName))
            {
                try
                {
                    await this._simpleKeyValueService.Add("", this._resourceName, "");
                }
                catch (Exception)
                {
                    //如果一场的话,就把他给释放掉
                    await this.ReleaseLock(lockId);
                    throw;
                }
                this._lockedResources.Add(this._resourceName, new HashSet<Guid>());
            }
            this._lockedResources[this._resourceName].Add(lockId);
            return this;
        }
    }
}
