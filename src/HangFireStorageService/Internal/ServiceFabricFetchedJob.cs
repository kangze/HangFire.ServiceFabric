using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hangfire.Common;
using Hangfire.ServiceFabric.Servces;
using Hangfire.Storage;
using HangFireStorageService.Dto;

namespace HangFireStorageService.Internal
{
    public class ServiceFabricFetchedJob : IFetchedJob
    {
        private readonly IJobQueueAppService _jobQueueAppService;

        private bool _disposed;

        private bool _removedFromQueue;

        private bool _requeued;

        /// <summary>
        /// Constructs fetched job by database connection, identifier, job ID and queue
        /// </summary>
        /// <param name="connection">Database connection</param>
        /// <param name="id">Identifier</param>
        /// <param name="jobId">Job ID</param>
        /// <param name="queue">Queue name</param>
        public ServiceFabricFetchedJob(string id, string jobId, string queue, IJobQueueAppService jobQueueAppService)
        {
            Id = id;
            JobId = jobId.ToString();
            Queue = queue ?? throw new ArgumentNullException(nameof(queue));
            this._jobQueueAppService = jobQueueAppService;
        }

        /// <summary>
        /// Job ID
        /// </summary>
        public string JobId { get; }

        public string Id { get; set; }

        /// <summary>
        /// Queue name
        /// </summary>
        public string Queue { get; }

        /// <summary>
        /// Removes fetched job from a queue
        /// </summary>
        public void RemoveFromQueue()
        {
            this._jobQueueAppService.DeleteQueueJobAsync(Id).GetAwaiter().GetResult();
            _removedFromQueue = true;
        }

        /// <summary>
        /// Puts fetched job into a queue
        /// </summary>
        public void Requeue()
        {
            var jobQueue = this._jobQueueAppService.GetQueueAsync(Id).GetAwaiter().GetResult();
            jobQueue.FetchedAt = null;
            this._jobQueueAppService.UpdateQueueAsync(jobQueue);
            _requeued = true;
        }

        /// <summary>
        /// Disposes the object
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            if (!_removedFromQueue && !_requeued)
            {
                Requeue();
            }

            _disposed = true;
        }
    }
}
