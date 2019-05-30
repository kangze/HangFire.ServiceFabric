using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire.Common;
using Hangfire.States;
using Hangfire.Storage;
using HangFireStorageService.Dto;
using HangFireStorageService.Servces;

namespace HangFireStorageService.Internal
{
    public class ServiceFabricWriteOnlyTransaction : JobStorageTransaction
    {

        private readonly IJobQueueAppService _jobQueueAppService;
        private readonly IJobAppService _jobAppService;
        private readonly IJobStateDataAppService _jobStateDataAppService;
        private readonly IServerAppService _serverAppService;
        private readonly ICounterAppService _counterAppService;
        private readonly IAggregatedCounterAppService _aggregatedCounterAppService;
        private readonly IJobSetAppService _jobSetAppService;
        private readonly IJobDataService _jobDataService;
        private readonly IHashAppService _hashAppService;
        private readonly IJobListAppService _jobListAppService;


        private readonly List<Action<IHashAppService>> _hashActions = new List<Action<IHashAppService>>();
        private readonly List<Action<IJobAppService>> _jobActions = new List<Action<IJobAppService>>();
        private readonly List<Action<IJobStateDataAppService>> _jobStateDataActions = new List<Action<IJobStateDataAppService>>();
        private readonly List<Action<IJobQueueAppService>> _jobQueueActions = new List<Action<IJobQueueAppService>>();
        private readonly List<Action<ICounterAppService>> _counterAppActions = new List<Action<ICounterAppService>>();
        private readonly List<Action<IJobSetAppService>> _jobSetAppActions = new List<Action<IJobSetAppService>>();
        private readonly List<Action<IJobListAppService>> _jobListAppActions = new List<Action<IJobListAppService>>();



        public ServiceFabricWriteOnlyTransaction(
            IJobQueueAppService jobQueueAppService,
            IJobAppService jobAppService,
            IJobStateDataAppService jobStateDataAppService,
            IServerAppService serverAppService,
            ICounterAppService counterAppService,
            IAggregatedCounterAppService aggregatedCounterAppService,
            IJobSetAppService jobSetAppService,
            IJobDataService jobDataService,
            IHashAppService hashAppService,
            IJobListAppService jobListAppService
            )
        {
            this._jobQueueAppService = jobQueueAppService;
            this._serverAppService = serverAppService;
            this._counterAppService = counterAppService;
            this._aggregatedCounterAppService = aggregatedCounterAppService;
            this._jobSetAppService = jobSetAppService;
            this._jobStateDataAppService = jobStateDataAppService;
            this._jobAppService = jobAppService;
            this._jobDataService = jobDataService;
            this._hashAppService = hashAppService;
            this._jobListAppService = jobListAppService;
        }

        public override void ExpireJob(string jobId, TimeSpan expireIn)
        {
            this._jobActions.Add((jobAppService) =>
            {
                var job = jobAppService.GetJobAsync(long.Parse(jobId)).GetAwaiter().GetResult();
                job.ExpireAt = DateTime.UtcNow.Add(expireIn);
                jobAppService.UpdateJobAsync(job).GetAwaiter().GetResult();
            });
        }

        public override void PersistJob(string jobId)
        {
            this._jobActions.Add((jobAppService) =>
            {
                var job = jobAppService.GetJobAsync(long.Parse(jobId)).GetAwaiter().GetResult();
                job.ExpireAt = null;
                jobAppService.UpdateJobAsync(job).GetAwaiter().GetResult();
            });
        }

        public override void SetJobState(string jobId, IState state)
        {
            this._jobActions.Add((jobAppService) =>
            {
                jobAppService.SetJobStateAsync(long.Parse(jobId), new StateDto()
                {
                    CreatedAt = DateTime.UtcNow,
                    Data = SerializationHelper.Serialize(state.SerializeData()),
                    Reason = state.Reason?.Substring(0, Math.Min(99, state.Reason.Length)),
                    Name = state.Name,
                    JobId = long.Parse(jobId)
                }).GetAwaiter().GetResult();
            });
        }

        public override void AddJobState(string jobId, IState state)
        {
            this._jobStateDataActions.Add((jobStateDataAppService) =>
            {
                jobStateDataAppService.AddStateAsync(long.Parse(jobId), new StateDto()
                {
                    CreatedAt = DateTime.UtcNow,
                    Data = SerializationHelper.Serialize(state.SerializeData()),
                    Reason = state.Reason?.Substring(0, Math.Min(99, state.Reason.Length)),
                    Name = state.Name,
                    JobId = long.Parse(jobId)
                });
            });
        }

        public override void AddToQueue(string queue, string jobId)
        {
            this._jobQueueActions.Add((jobQueueAppService) =>
            {
                jobQueueAppService.AddToQueueJObAsync(queue, long.Parse(jobId)).GetAwaiter().GetResult();
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
                counterAppService.DeleteAsync(key, null).GetAwaiter().GetResult();

            });
        }

        public override void DecrementCounter(string key, TimeSpan expireIn)
        {
            this._counterAppActions.Add((counterAppService) =>
            {
                counterAppService.DeleteAsync(key, expireIn).GetAwaiter().GetResult();

            });
        }

        public override void AddToSet(string key, string value)
        {
            this._jobSetAppActions.Add((jobSetAppService) =>
            {
                jobSetAppService.AddSetAsync(key, value, 0.0);
            });
        }

        public override void AddToSet(string key, string value, double score)
        {
            this._jobSetAppActions.Add((jobSetAppService) =>
            {
                jobSetAppService.AddSetAsync(key, value, score);
            });
        }

        public override void RemoveFromSet(string key, string value)
        {
            this._jobSetAppActions.Add((jobSetAppService) =>
            {
                jobSetAppService.RemoveAsync(key, value);
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
                jobListAppService.Remove(key, value);
            });
        }

        public override void TrimList(string key, int keepStartingFrom, int keepEndingAt)
        {
            this._jobListAppActions.Add((jobListAppService) =>
            {
                jobListAppService.RemoveRange(key, keepStartingFrom, keepEndingAt);
            });
        }

        public override void SetRangeInHash(string key, IEnumerable<KeyValuePair<string, string>> keyValuePairs)
        {
            this._hashActions.Add((hashAppService) =>
            {
                var hashs = hashAppService.GetAllHashAsync().GetAwaiter().GetResult();
                if (hashs.Where(u => u.Key == key).Count() == 0)
                {
                    //add it
                    var dict = keyValuePairs.ToDictionary(u => u.Key, u => u.Value);
                    hashAppService.AddOrUpdateAsync(key, dict).GetAwaiter().GetResult();
                }
                else
                {

                    foreach (var pair in keyValuePairs)
                    {
                        var currentHash = hashs.FirstOrDefault(u => u.Key == pair.Key);
                        if (currentHash != null && currentHash.Value == pair.Value)
                            continue;
                        else if (currentHash != null)
                            hashAppService.AddOrUpdateAsync(key, new Dictionary<string, string>() { { pair.Key, pair.Value } });

                    }
                }
            });
        }

        public override void RemoveHash(string key)
        {
            this._hashActions.Add((hashAppService) =>
            {
                hashAppService.RemoveAsync(key).GetAwaiter().GetResult();
            });
        }

        public override void Commit()
        {
            this._hashActions.ForEach(u => u.Invoke(this._hashAppService));
            this._jobActions.ForEach(u => u.Invoke(this._jobAppService));
            this._jobStateDataActions.ForEach(u => u.Invoke(this._jobStateDataAppService));
            this._jobQueueActions.ForEach(u => u.Invoke(this._jobQueueAppService));
            this._counterAppActions.ForEach(u => u.Invoke(this._counterAppService));
            this._jobSetAppActions.ForEach(u => u.Invoke(this._jobSetAppService));
            this._jobListAppActions.ForEach(u => u.Invoke(this._jobListAppService));
        }
    }
}
