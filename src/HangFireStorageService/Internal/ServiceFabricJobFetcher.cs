using Hangfire.ServiceFabric.Servces;
using Hangfire.Storage;
using HangFireStorageService.Dto;
using HangFireStorageService.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hangfire.ServiceFabric.Internal
{
    public class ServiceFabricJobFetcher
    {

        private readonly IJobQueueAppService _jobQueueAppService;
        private readonly DateTime _invisibilityTimeout;
        public static readonly AutoResetEvent NewItemInQueueEvent = new AutoResetEvent(false);


        public ServiceFabricJobFetcher(IJobQueueAppService jobQueueAppService)
        {
            this._invisibilityTimeout = DateTime.UtcNow.AddMinutes(30);
            this._jobQueueAppService = jobQueueAppService;
        }


        public IFetchedJob FetchNextJob(string[] queues, CancellationToken cancellationToken)
        {
            if (queues == null)
            {
                throw new ArgumentNullException(nameof(queues));
            }

            if (queues.Length == 0)
            {
                throw new ArgumentException("Queue array must be non-empty.", nameof(queues));
            }

            ServiceFabricFetchedJob fetchedJob = null;

            while (fetchedJob == null)
            {
                cancellationToken.ThrowIfCancellationRequested();
                fetchedJob = TryAllQueues(queues, cancellationToken);

                if (fetchedJob != null) return fetchedJob;
                fetchedJob = TryGetEnqueuedJob("", cancellationToken);
            }

            return fetchedJob;
        }

        private ServiceFabricFetchedJob TryAllQueues(string[] queues, CancellationToken cancellationToken)
        {
            foreach (var queue in queues)
            {
                var fetchedJob = TryGetEnqueuedJob(queue, cancellationToken);
                if (fetchedJob == null)
                {
                    continue;
                }
                return fetchedJob;
            }
            return null;
        }

        private ServiceFabricFetchedJob TryGetEnqueuedJob(string queue, CancellationToken cancellationToken)
        {
            JobQueueDto fetchedJob = null;
            do
            {
                var jobs = this._jobQueueAppService.GetQueuesAsync(queue).GetAwaiter().GetResult();
                fetchedJob = (from job in jobs
                              where job.FetchedAt == null || job.FetchedAt.Value <= _invisibilityTimeout
                              select job).FirstOrDefault();
                if (fetchedJob == null)
                {
                    WaitHandle.WaitAny(new WaitHandle[] { cancellationToken.WaitHandle, NewItemInQueueEvent }, 2000);
                    continue;
                }
                fetchedJob.FetchedAt = DateTime.Now;
                this._jobQueueAppService.UpdateQueueAsync(fetchedJob);
                break;
            } while (true);

            return new ServiceFabricFetchedJob(fetchedJob.Id, fetchedJob.JobId, queue, _jobQueueAppService);
        }
    }
}
