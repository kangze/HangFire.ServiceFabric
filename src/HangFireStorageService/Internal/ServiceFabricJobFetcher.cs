using Hangfire.Storage;
using HangFireStorageService.Internal;
using HangFireStorageService.Servces;
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


        public ServiceFabricJobFetcher(IJobQueueAppService jobQueueAppService)
        {
            this._invisibilityTimeout = DateTime.UtcNow.AddSeconds(13);
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


                //if (_semaphore.WaitAny(queues, cancellationToken, _storageOptions.QueuePollInterval, out var queue))
                //{

                //}
                //TODO:
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
                // make sure to try to decrement semaphore if we succeed in getting a job from the queue
                //_semaphore.WaitNonBlock(queue);
                return fetchedJob;
            }

            return null;
        }

        private ServiceFabricFetchedJob TryGetEnqueuedJob(string queue, CancellationToken cancellationToken)
        {
            var jobs = this._jobQueueAppService.GetQueuesAsync(queue).GetAwaiter().GetResult();
            var fetchedJob = (from job in jobs
                              where job.FetchedAt == null || job.FetchedAt.Value <= _invisibilityTimeout
                              select job).FirstOrDefault();
            if (fetchedJob == null)
            {
                return null;
            }
            fetchedJob.FetchedAt = DateTime.Now;
            this._jobQueueAppService.UpdateQueueAsync(fetchedJob);

            return new ServiceFabricFetchedJob(fetchedJob.Id, fetchedJob.JobId,queue, _jobQueueAppService);
        }
    }
}
