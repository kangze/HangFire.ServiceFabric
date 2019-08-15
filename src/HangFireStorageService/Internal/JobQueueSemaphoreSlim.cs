using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hangfire.ServiceFabric.Internal
{
    public class JobQueueSemaphoreSlim
    {
        public static JobQueueSemaphoreSlim Instance = new JobQueueSemaphoreSlim();
        public static object obj = new object();
        public static IDictionary<string, SemaphoreSlim> Slims = new Dictionary<string, SemaphoreSlim>();

        public void Wait(string resource)
        {
            SemaphoreSlim slim = null;
            lock (obj)
            {
                if (!Slims.ContainsKey(resource))
                {
                    slim = new SemaphoreSlim(1, 1);
                    Slims.Add(resource, slim);
                }
                slim = Slims[resource];
            }
            System.Diagnostics.Debug.WriteLine("slim Wait Begin CurrentThreadId:" + Thread.CurrentThread.ManagedThreadId);
            slim.Wait();
            System.Diagnostics.Debug.WriteLine("slim Wait Gain CurrentThreadId:" + Thread.CurrentThread.ManagedThreadId);
        }

        public void Relase(string resource)
        {
            if (Slims.ContainsKey(resource))
                Slims[resource].Release();
        }
    }
}
