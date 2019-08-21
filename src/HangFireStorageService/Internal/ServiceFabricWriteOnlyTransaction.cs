using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hangfire.Common;
using Hangfire.ServiceFabric.Dtos;
using Hangfire.ServiceFabric.Internal;
using Hangfire.ServiceFabric.Model;
using Hangfire.ServiceFabric.Model.Dtos;
using Hangfire.ServiceFabric.Model.Interfaces;
using Hangfire.States;
using Hangfire.Storage;
using HangFireStorageService.Dto;

namespace Hangfire.ServiceFabric.Internal
{
    public class ServiceFabricWriteOnlyTransaction : JobStorageTransaction
    {
        private readonly IServiceFabriceStorageServices _services;

        private readonly List<Action<IHashAppService>> _hashActions = new List<Action<IHashAppService>>();
        private readonly List<Action<IJobAppService>> _jobActions = new List<Action<IJobAppService>>();
        private readonly List<Action<IJobQueueAppService>> _jobQueueActions = new List<Action<IJobQueueAppService>>();
        private readonly List<Action<ICounterAppService>> _counterAppActions = new List<Action<ICounterAppService>>();
        private readonly List<Action<IJobSetAppService>> _jobSetAppActions = new List<Action<IJobSetAppService>>();
        private readonly List<Action<IListAppService>> _jobListAppActions = new List<Action<IListAppService>>();


        public ServiceFabricWriteOnlyTransaction(IServiceFabriceStorageServices servies)
        {
            this._services = servies;
        }

        public override void ExpireJob(string jobId, TimeSpan expireIn)
        {
            this._jobActions.Add((jobAppService) =>
            {
                var job = jobAppService.GetJobAsync(jobId).GetAwaiter().GetResult();
                job.ExpireAt = DateTime.UtcNow.Add(expireIn);
                jobAppService.AddOrUpdateAsync(job).GetAwaiter().GetResult();
            });
        }

        public string CreateExpiredJob(Job job, IDictionary<string, string> parameters, DateTime createdAt,
            TimeSpan expireIn)
        {
            if (job == null)
                throw new ArgumentNullException(nameof(job));

            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            var invocationData = InvocationData.Serialize(job);

            var jobDto = new JobDto
            {
                Id = Guid.NewGuid().ToString("N"),
                InvocationData = Hangfire.Common.JobHelper.ToJson(invocationData),
                Arguments = invocationData.Arguments,
                Parameters = parameters.ToDictionary(kv => kv.Key, kv => kv.Value),
                CreatedAt = createdAt,
                ExpireAt = createdAt.Add(expireIn)
            };

            this._jobActions.Add((jobAppService) =>
            {
                jobAppService.AddOrUpdateAsync(jobDto).GetAwaiter().GetResult();
            });

            return jobDto.Id;
        }

        public async Task SetJobParameter(string id, string name, string value)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            var jobDto = this._services.JobAppService.GetJobAsync(id).GetAwaiter().GetResult();
            if (jobDto == null)
                return;
            if (jobDto.Parameters.ContainsKey(name))
                jobDto.Parameters[name] = value;
            else
                jobDto.Parameters.Add(name, value);
            this._jobActions.Add((jobAppService) =>
            {
                jobAppService.AddOrUpdateAsync(jobDto).GetAwaiter().GetResult();
            });
        }

        public override void PersistJob(string jobId)
        {
            this._jobActions.Add((jobAppService) =>
            {
                var job = jobAppService.GetJobAsync(jobId).GetAwaiter().GetResult();
                job.ExpireAt = null;
                jobAppService.AddOrUpdateAsync(job).GetAwaiter().GetResult();
            });
        }

        public override void SetJobState(string jobId, IState state)
        {
            this._jobActions.Add((jobAppService) =>
            {
                var job = jobAppService.GetJobAsync(jobId).GetAwaiter().GetResult();
                var stateDto = new StateDto
                {
                    Name = state.Name,
                    Reason = state.Reason,
                    CreatedAt = DateTime.UtcNow,
                    Data = state.SerializeData()
                };
                job.StateHistory.Add(stateDto);
                job.StateName = state.Name;
                jobAppService.AddOrUpdateAsync(job).GetAwaiter().GetResult();
            });
        }

        public override void AddJobState(string jobId, IState state)
        {
            this._jobActions.Add((jobAppService) =>
            {
                var job = jobAppService.GetJobAsync(jobId).GetAwaiter().GetResult();
                var stateDto = new StateDto
                {
                    Name = state.Name,
                    Reason = state.Reason,
                    CreatedAt = DateTime.UtcNow,
                    Data = state.SerializeData()
                };
                job.StateHistory.Add(stateDto);
                jobAppService.AddOrUpdateAsync(job).GetAwaiter().GetResult();
            });
        }

        public override void AddToQueue(string queue, string jobId)
        {
            this._jobQueueActions.Add((jobQueueAppService) =>
            {
                jobQueueAppService.AddToQueueJObAsync(queue, jobId).GetAwaiter().GetResult();
                ServiceFabricStorageConnection.AutoResetNewEvent.Set();
                //ServiceFabricJobFetcher.NewItemInQueueEvent.Set();
            });
        }

        public override void IncrementCounter(string key)
        {
            this._counterAppActions.Add((counterAppService) =>
            {
                counterAppService.AddAsync(key, null).GetAwaiter().GetResult();

            });
        }

        public override void IncrementCounter(string key, TimeSpan expireIn)
        {
            this._counterAppActions.Add((counterAppService) =>
            {
                counterAppService.AddAsync(key, expireIn).GetAwaiter().GetResult();

            });
        }

        public override void DecrementCounter(string key)
        {
            this._counterAppActions.Add((counterAppService) =>
            {
                counterAppService.DecrementAsync(key, -1, null).GetAwaiter().GetResult();

            });
        }

        public override void DecrementCounter(string key, TimeSpan expireIn)
        {
            this._counterAppActions.Add((counterAppService) =>
            {
                counterAppService.DecrementAsync(key, -1, expireIn).GetAwaiter().GetResult();

            });
        }

        public override void AddToSet(string key, string value)
        {
            this._jobSetAppActions.Add((jobSetAppService) =>
            {
                jobSetAppService.AddSetAsync(key, value, 0.0).GetAwaiter().GetResult();
            });
        }

        public override void AddToSet(string key, string value, double score)
        {
            this._jobSetAppActions.Add((jobSetAppService) =>
            {
                jobSetAppService.AddSetAsync(key, value, score).GetAwaiter().GetResult();
            });
        }

        public override void RemoveFromSet(string key, string value)
        {
            this._jobSetAppActions.Add((jobSetAppService) =>
            {
                if (key.Contains("w"))
                {
                    var s = 10;
                }
                jobSetAppService.RemoveAsync(key, value).GetAwaiter().GetResult();
            });
        }

        public override void InsertToList(string key, string value)
        {
            this._jobListAppActions.Add((jobListAppService) =>
            {
                jobListAppService.AddAsync(key, value).GetAwaiter().GetResult();
            });
        }

        public override void RemoveFromList(string key, string value)
        {
            this._jobListAppActions.Add((jobListAppService) =>
            {
                jobListAppService.Remove(key, value).GetAwaiter().GetResult();
            });
        }

        public override void TrimList(string key, int keepStartingFrom, int keepEndingAt)
        {
            this._jobListAppActions.Add((jobListAppService) =>
            {
                jobListAppService.RemoveRange(key, keepStartingFrom, keepEndingAt).GetAwaiter().GetResult();
            });
        }

        public override void SetRangeInHash(string key, IEnumerable<KeyValuePair<string, string>> keyValuePairs)
        {
            this._hashActions.Add((hashAppService) =>
            {
                var hashDto = hashAppService.GetHashDtoAsync(key).GetAwaiter().GetResult();
                var fields = hashDto == null ? new Dictionary<string, string>() : hashDto.Fields;
                foreach (var kv in keyValuePairs)
                {
                    fields.TryAdd(kv.Key, kv.Value);
                }
                var dto = new HashDto()
                {
                    Id = hashDto == null ? Guid.NewGuid().ToString("N") : hashDto.Id,
                    Key = key,
                    ExpireAt = null,
                    Fields = fields
                };
                hashAppService.AddOrUpdateAsync(dto).GetAwaiter().GetResult();
            });
        }

        public override void RemoveHash(string key)
        {
            this._hashActions.Add((hashAppService) =>
            {
                hashAppService.RemoveAsync(key).GetAwaiter().GetResult();
            });
        }

        public static bool process = true;

        public override void Commit()
        {
            while (!process)
            {
                Thread.Sleep(100);
            }
            process = false;
            this._hashActions.ForEach(u => u.Invoke(this._services.HashAppService));
            this._jobActions.ForEach(u => u.Invoke(this._services.JobAppService));
            this._jobQueueActions.ForEach(u => u.Invoke(this._services.JobQueueAppService));
            this._counterAppActions.ForEach(u => u.Invoke(this._services.CounterAppService));
            this._jobSetAppActions.ForEach(u => u.Invoke(this._services.JobSetAppService));
            this._jobListAppActions.ForEach(u => u.Invoke(this._services.ListAppService));
            process = true;
        }
    }
}
