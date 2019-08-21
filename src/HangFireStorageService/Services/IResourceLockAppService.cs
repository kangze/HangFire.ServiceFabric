using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.ServiceFabric.Services
{
    public interface IResourceLockAppService:IService
    {
        Task<bool> LockAsync(string resource);

        Task<bool> ReleaseAsync(string resource);
    }
}
