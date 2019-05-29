using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hangfire.Common;
using Hangfire.Storage;
using HangFireStorageService.Dto;
using HangFireStorageService.Servces;

namespace HangFireStorageService.Internal
{
    public class ServiceFabricTransactionJob : IFetchedJob
    {
        private readonly string[] _queues;
        private readonly CancellationToken _cancelToken;
        private readonly IJobQueueAppService _jobQueueAppService;
        private readonly JobQueueDto _currentJobQueueDto;


        public ServiceFabricTransactionJob(
            string[] queues,
            CancellationToken cancellationToken,
            IJobQueueAppService jobQueueAppService
            )
        {
            this._queues = queues;
            this._cancelToken = cancellationToken;
            this._jobQueueAppService = jobQueueAppService;
            //这里必须预先出队
            this._currentJobQueueDto = this.Dequeue();
        }

        private JobQueueDto Dequeue()
        {
            //这里的意思就是在众多的queue中进行出队的操作
            using (var cancellationEvent = _cancelToken.GetCancellationEvent())
            {
                do
                {
                    foreach (var queue in this._queues)
                    {
                        var job_queue = this._jobQueueAppService.GetQueuesAsync(queue).GetAwaiter().GetResult();
                        var dequeue = job_queue.
                            Where(u => u.FetchedAt == null || u.FetchedAt.Value.ToUniversalTime() < DateTime.UtcNow.AddSeconds(13)).ToList();
                        if (dequeue.Count != 0)
                            return dequeue[0];
                        continue;

                    }
                    _cancelToken.ThrowIfCancellationRequested();
                    Thread.Sleep(3000);
                } while (true);
            }
        }

        public string JobId
        {
            get
            {
                return this._currentJobQueueDto.JobId.ToString();
            }
        }

        public void Dispose()
        {
            //do not anythins
        }

        public void RemoveFromQueue()
        {
            this._jobQueueAppService.DeleteQueueJobAsync(this._currentJobQueueDto.Queue, this._currentJobQueueDto.JobId);
        }

        public void Requeue()
        {
            // do not any things
        }
    }
}
