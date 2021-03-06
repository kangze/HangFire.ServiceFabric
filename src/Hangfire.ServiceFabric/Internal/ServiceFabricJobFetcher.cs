﻿using Hangfire.ServiceFabric.Dtos;
using Hangfire.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hangfire.ServiceFabric.Model.Dtos;
using Hangfire.ServiceFabric.Model.Interfaces;

namespace Hangfire.ServiceFabric.Internal
{
    public class ServiceFabricJobFetcher
    {
        private readonly IJobQueueAppService _jobQueueAppService;
        private readonly DateTime _invisibilityTimeout;
        private readonly object lock_obj = new object();


        public ServiceFabricJobFetcher(IJobQueueAppService jobQueueAppService)
        {
            this._invisibilityTimeout = DateTime.UtcNow.AddMinutes(30);
            this._jobQueueAppService = jobQueueAppService;
        }


        public IFetchedJob FetchNextJob(string[] queues, CancellationToken cancellationToken)
        {
            System.Diagnostics.Debug.WriteLine("FetchNextJob CurrentThreadId:" + Thread.CurrentThread.ManagedThreadId);
            while (true)
            {
                foreach (var queue in queues)
                {
                    JobQueueSemaphoreSlim.Instance.Wait(queue);
                    var fetchedJob = this._jobQueueAppService.GetFetchedJobAsync(queue).GetAwaiter().GetResult();
                    if (fetchedJob == null)
                        continue;
                    fetchedJob.FetchedAt = DateTime.Now;
                    System.Diagnostics.Debug.WriteLine("UpdateFetchedJob.FetchEdAt Thread Id:" + Thread.CurrentThread.ManagedThreadId);
                    this._jobQueueAppService.UpdateQueueAsync(fetchedJob).GetAwaiter().GetResult();
                    JobQueueSemaphoreSlim.Instance.Relase(queue);
                    System.Diagnostics.Debug.WriteLine("Slim Release CurrentThreadId:" + Thread.CurrentThread.ManagedThreadId);
                    return new ServiceFabricFetchedJob(fetchedJob.Id, fetchedJob.JobId, queue, _jobQueueAppService);
                }
                Thread.Sleep(1000);
            }
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
                    //WaitHandle.WaitAny(new WaitHandle[] { cancellationToken.WaitHandle, NewItemInQueueEvent }, 2000);
                    Thread.Sleep(200);
                    continue;
                }
                fetchedJob.FetchedAt = DateTime.Now;
                lock (lock_obj)
                {
                    this._jobQueueAppService.UpdateQueueAsync(fetchedJob);
                }
                break;
            } while (true);

            return new ServiceFabricFetchedJob(fetchedJob.Id, fetchedJob.JobId, queue, _jobQueueAppService);
        }
    }
}
