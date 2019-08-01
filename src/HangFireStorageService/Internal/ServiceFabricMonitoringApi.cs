using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire.Common;
using Hangfire.ServiceFabric.Dtos;
using Hangfire.ServiceFabric.Dtos.Internal;
using Hangfire.ServiceFabric.Entities;
using Hangfire.States;
using Hangfire.Storage;
using Hangfire.Storage.Monitoring;
using HangFireStorageService.Dto;
using HangFireStorageService.Servces;

namespace HangFireStorageService.Internal
{
    public class ServiceFabricMonitoringApi : IMonitoringApi
    {
        private readonly IJobQueueAppService _jobQueueAppService;
        private readonly IJobAppService _jobAppService;
        private readonly IServerAppService _serverAppService;
        private readonly ICounterAppService _counterAppService;
        private readonly IAggregatedCounterAppService _aggregatedCounterAppService;
        private readonly IJobSetAppService _jobSetAppService;

        public ServiceFabricMonitoringApi(
            IJobQueueAppService jobQueueAppService,
            IJobAppService jobAppService,
            IServerAppService serverAppService,
            ICounterAppService counterAppService,
            IAggregatedCounterAppService aggregatedCounterAppService,
            IJobSetAppService jobSetAppService
            )
        {
            this._jobQueueAppService = jobQueueAppService;
            this._serverAppService = serverAppService;
            this._counterAppService = counterAppService;
            this._aggregatedCounterAppService = aggregatedCounterAppService;
            this._jobSetAppService = jobSetAppService;
            this._jobAppService = jobAppService;
        }

        public IList<QueueWithTopEnqueuedJobsDto> Queues()
        {

            var job_queueus = this._jobQueueAppService.GetQueuesAsync(null).GetAwaiter().GetResult();
            var result = new List<QueueWithTopEnqueuedJobsDto>(job_queueus.Count);
            //需要根据queue进行一次分组
            var groupd_queues = job_queueus.GroupBy(u => u.Queue);
            foreach (var queue in groupd_queues)
            {
                var enqueueJobs = this._jobQueueAppService.GetEnqueuedJob(queue.Key, 0, 5).GetAwaiter().GetResult();
                var enqueueJobIds = enqueueJobs.Select(u => u.JobId).ToList();


                var counters = this._jobQueueAppService.GetEnqueuedAndFetchedCount(queue.Key).GetAwaiter().GetResult();

                result.Add(new QueueWithTopEnqueuedJobsDto()
                {
                    Name = queue.Key,
                    Length = counters.EnqueuedCount ?? 0,
                    Fetched = counters.FetchedCount,
                    FirstJobs = EnqueuedJobs(enqueueJobIds)
                });
            }
            return result;
        }

        public IList<ServerDto> Servers()
        {
            var servers = this._serverAppService.GetAllServerAsync().GetAwaiter().GetResult();
            return servers.Select(server =>
            {
                var data = SerializationHelper.Deserialize<ServerData>(server.Data);
                return new ServerDto()
                {
                    Name = server.ServerId,
                    Heartbeat = server.LastHeartbeat,
                    Queues = data.Queues,
                    StartedAt = data.StartedAt ?? DateTime.MinValue,
                    WorkersCount = data.WorkCount
                };
            }).ToList();
        }

        public JobDetailsDto JobDetails(string jobId)
        {
            var job = this._jobAppService.GetJobAsync(jobId).GetAwaiter().GetResult();

            var history = job.StateHistory.Select(u => new StateHistoryDto()
            {
                StateName = u.Name,
                CreatedAt = u.CreatedAt,
                Reason = u.Reason,
                Data = u.Data
            })
            .Reverse()
            .ToList();
            var result = new JobDetailsDto()
            {
                CreatedAt = job.CreatedAt,
                ExpireAt = job.ExpireAt,
                Properties = job.Parameters,
                History = history,
                Job = DeserializeJob(job.InvocationData, job.Arguments)
            };
            return result;
        }

        private static readonly string[] StatisticsStateNames = new[]
       {
            EnqueuedState.StateName,
            FailedState.StateName,
            ProcessingState.StateName,
            ScheduledState.StateName
        };

        public StatisticsDto GetStatistics()
        {

            var stats = new StatisticsDto();
            var all_job = this._jobAppService.GetJobsAsync().GetAwaiter().GetResult();
            var countByStates = all_job
                .Where(u => StatisticsStateNames.Contains(u.StateName))
                .GroupBy(u => u.StateName)
                .ToDictionary(u => u.Key, u => u.Count());

            int GetCountIfExists(string name) => countByStates.ContainsKey(name) ? countByStates[name] : 0;

            stats.Enqueued = GetCountIfExists(EnqueuedState.StateName);
            stats.Failed = GetCountIfExists(FailedState.StateName);
            stats.Processing = GetCountIfExists(ProcessingState.StateName);
            stats.Scheduled = GetCountIfExists(ScheduledState.StateName);

            var all_server = this._serverAppService.GetAllServerAsync().GetAwaiter().GetResult();
            stats.Servers = all_server.Count();

            var all_counter = this._counterAppService.GetAllCounterAsync().GetAwaiter().GetResult();

            var statsSucceeded = $@"stats:{State.Succeeded}";
            var successCounter = all_counter.FirstOrDefault(u => u.Key == statsSucceeded);
            stats.Succeeded = successCounter?.Value ?? 0;

            var statsDeleted = $@"stats:{State.Deleted}";
            var deletedCounter = all_counter.FirstOrDefault(u => u.Key == statsDeleted);
            stats.Deleted = deletedCounter?.Value ?? 0;

            var all_sets = this._jobSetAppService.GetSetsAsync().GetAwaiter().GetResult();
            stats.Recurring = all_sets.Count(u => u.Key.Contains("recurring-jobs"));

            var queues = this._jobQueueAppService.GetQueuesAsync(null).GetAwaiter().GetResult();
            stats.Queues = queues.Count;

            return stats;
        }

        public JobList<EnqueuedJobDto> EnqueuedJobs(string queue, int @from, int perPage)
        {
            var enqueuedJobs = this._jobQueueAppService.GetEnqueuedJob(queue, from, perPage).GetAwaiter().GetResult();
            return this.EnqueuedJobs(enqueuedJobs.Select(u => u.JobId).ToArray());
        }

        public JobList<FetchedJobDto> FetchedJobs(string queue, int @from, int perPage)
        {
            var fetchedJobIds = this._jobQueueAppService.GetFetchedJobIds(queue, from, perPage).GetAwaiter().GetResult();

            return FetchedJobs(fetchedJobIds);

        }

        public JobList<ProcessingJobDto> ProcessingJobs(int @from, int count)
        {
            return this.GetJobs(from, count, ProcessingState.StateName, (jobDto, job, stateData) =>
             {
                 return new ProcessingJobDto
                 {
                     Job = job,
                     InProcessingState = ProcessingState.StateName.Equals(jobDto.StateName, StringComparison.OrdinalIgnoreCase),
                     ServerId = stateData.ContainsKey("ServerId") ? stateData["ServerId"] : stateData["ServerName"],
                     StartedAt = jobDto.CreatedAt,
                 };
             });
        }

        public JobList<ScheduledJobDto> ScheduledJobs(int @from, int count)
        {
            return this.GetJobs(from, count, ScheduledState.StateName, (jobDto, job, stateData) =>
            {
                return new ScheduledJobDto
                {
                    Job = job,
                    InScheduledState = ScheduledState.StateName.Equals(jobDto.StateName, StringComparison.OrdinalIgnoreCase),
                    EnqueueAt = Hangfire.Common.JobHelper.DeserializeNullableDateTime(stateData["EnqueueAt"]) ?? DateTime.MinValue,
                    ScheduledAt = jobDto.CreatedAt,
                };
            });
        }

        public JobList<SucceededJobDto> SucceededJobs(int @from, int count)
        {
            return this.GetJobs(from, count, SucceededState.StateName, (jobDto, job, stateData) =>
            {
                return new SucceededJobDto
                {
                    Job = job,
                    InSucceededState = SucceededState.StateName.Equals(jobDto.StateName, StringComparison.OrdinalIgnoreCase),
                    Result = stateData["Result"],
                    TotalDuration = stateData.ContainsKey("PerformanceDuration") && stateData.ContainsKey("Latency")
                        ? (long?)long.Parse(stateData["PerformanceDuration"]) + (long?)long.Parse(stateData["Latency"])
                        : null,
                    SucceededAt = jobDto.CreatedAt,
                };
            });
        }

        public JobList<FailedJobDto> FailedJobs(int @from, int count)
        {
            return this.GetJobs(from, count, FailedState.StateName, (jobDto, job, stateData) =>
            {
                return new FailedJobDto
                {
                    Job = job,
                    InFailedState = FailedState.StateName.Equals(jobDto.StateName, StringComparison.OrdinalIgnoreCase),
                    //Reason = jobDto.StateReason, //缺失这个属性,TODO后续补上
                    ExceptionDetails = stateData["ExceptionDetails"],
                    ExceptionMessage = stateData["ExceptionMessage"],
                    ExceptionType = stateData["ExceptionType"],
                    FailedAt = jobDto.CreatedAt
                };
            });
        }

        public JobList<DeletedJobDto> DeletedJobs(int @from, int count)
        {
            return this.GetJobs(from, count, DeletedState.StateName, (jobDto, job, stateData) =>
            {
                return new DeletedJobDto
                {
                    Job = job,
                    InDeletedState = DeletedState.StateName.Equals(jobDto.StateName, StringComparison.OrdinalIgnoreCase),
                    DeletedAt = jobDto.CreatedAt
                };
            });
        }

        public long ScheduledCount()
        {
            return GetNumberOfJobsByStateName(ScheduledState.StateName);
        }

        public long EnqueuedCount(string queue)
        {
            var job_queueus = this._jobQueueAppService.GetQueuesAsync(queue).GetAwaiter().GetResult();
            return job_queueus.Aggregate(0, (t, next) =>
            {
                if (next.FetchedAt == null) return ++t;
                return 0;
            });
        }

        public long FetchedCount(string queue)
        {
            var job_queueus = this._jobQueueAppService.GetQueuesAsync(queue).GetAwaiter().GetResult();
            return job_queueus.Aggregate(0, (t, next) =>
            {
                if (next.FetchedAt != null) return ++t;
                return 0;
            });
        }

        public long FailedCount()
        {
            return GetNumberOfJobsByStateName(FailedState.StateName);
        }

        public long ProcessingCount()
        {
            return GetNumberOfJobsByStateName(ProcessingState.StateName);
        }

        public long SucceededListCount()
        {
            return GetNumberOfJobsByStateName(SucceededState.StateName);
        }

        public long DeletedListCount()
        {
            return GetNumberOfJobsByStateName(DeletedState.StateName);
        }

        public IDictionary<DateTime, long> SucceededByDatesCount()
        {
            return this.GetTimelineStats("succeeded");
        }

        public IDictionary<DateTime, long> FailedByDatesCount()
        {
            return this.GetTimelineStats("failed");
        }

        public IDictionary<DateTime, long> HourlySucceededJobs()
        {
            return this.GetHourlyTimelineStats("succeeded");
        }

        public IDictionary<DateTime, long> HourlyFailedJobs()
        {
            return this.GetHourlyTimelineStats("failed");
        }

        private Dictionary<DateTime, long> GetHourlyTimelineStats(string type)
        {
            var endDate = DateTime.UtcNow;
            var dates = new List<DateTime>();
            for (var i = 0; i < 24; i++)
            {
                dates.Add(endDate);
                endDate = endDate.AddHours(-1);
            }

            var keyMaps = dates.ToDictionary(x => $"stats:{type}:{x.ToString("yyyy-MM-dd-HH")}", x => x);
            return GetTimelineStats(keyMaps);
        }

        private Dictionary<DateTime, long> GetTimelineStats(string type)
        {
            var endDate = DateTime.UtcNow.Date;
            var dates = new List<DateTime>();
            for (var i = 0; i < 7; i++)
            {
                dates.Add(endDate);
                endDate = endDate.AddDays(-1);
            }

            var keyMaps = dates.ToDictionary(x => $"stats:{type}:{x.ToString("yyyy-MM-dd")}", x => x);

            return GetTimelineStats(keyMaps);
        }


        private Dictionary<DateTime, long> GetTimelineStats(IDictionary<string, DateTime> keyMaps)
        {
            var all_aggregatedCounter = this._aggregatedCounterAppService.GetAllCounterAsync().GetAwaiter().GetResult();
            var valuesMap = all_aggregatedCounter.Where(u => keyMaps.Keys.Contains(u.Key)).ToDictionary(u => u.Key, u => u.Value);
            foreach (var key in keyMaps.Keys)
            {
                if (!valuesMap.ContainsKey(key)) valuesMap.Add(key, 0);
            }
            var result = new Dictionary<DateTime, long>();
            for (var i = 0; i < keyMaps.Count; i++)
            {
                var value = valuesMap[keyMaps.ElementAt(i).Key];
                result.Add(keyMaps.ElementAt(i).Value, value);
            }
            return result;
        }

        private JobList<TDto> GetJobs<TDto>(
        int from,
        int count,
        string stateName,
        Func<JobSummary, Job, Dictionary<string, string>, TDto> selector)
        {
            var all_jobs = this._jobAppService.GetJobsAsync().GetAwaiter().GetResult();

            var jobs = all_jobs.Where(u => u.StateName == stateName).ToList();
            jobs = jobs.OrderBy(u => u.Id).Skip(from).Take(count).ToList();

            var joinedJobs = jobs
                .Select(job =>
                {
                    var state = job.StateHistory.LastOrDefault(s => s.Name == stateName);
                    return new JobSummary
                    {
                        Id = job.Id.ToString(),
                        InvocationData = job.InvocationData,
                        Arguments = job.Arguments,
                        CreatedAt = job.CreatedAt,
                        ExpireAt = job.ExpireAt,
                        FetchedAt = null,
                        StateName = job.StateName,
                        StateReason = state?.Reason,
                        StateData = state?.Data
                    };
                })
                .ToList();

            return DeserializeJobs(joinedJobs, selector);
        }

        private JobList<FetchedJobDto> FetchedJobs(IEnumerable<string> jobIds)
        {
            var jobs = this._jobAppService.GetJobsByIdsAsync(jobIds.ToArray()).GetAwaiter().GetResult();
            List<JobSummary> joinedJobs = jobs
                .Select(job =>
                {
                    var state = job.StateHistory.LastOrDefault(s => s.Name == job.StateName);
                    return new JobSummary
                    {
                        Id = job.Id.ToString(),
                        InvocationData = job.InvocationData,
                        Arguments = job.Arguments,
                        CreatedAt = job.CreatedAt,
                        ExpireAt = job.ExpireAt,
                        FetchedAt = null,
                        StateName = job.StateName,
                        StateReason = state?.Reason,
                        StateData = state?.Data
                    };
                })
                .ToList();
            var result = new List<KeyValuePair<string, FetchedJobDto>>(joinedJobs.Count);

            foreach (var job in joinedJobs)
            {
                result.Add(new KeyValuePair<string, FetchedJobDto>(job.Id,
                    new FetchedJobDto
                    {
                        Job = DeserializeJob(job.InvocationData, job.Arguments),
                        State = job.StateName,
                        FetchedAt = job.FetchedAt
                    }));
            }
            return new JobList<FetchedJobDto>(result);

        }

        private JobList<EnqueuedJobDto> EnqueuedJobs(IEnumerable<string> jobIds)
        {
            var jobDetails = this._jobAppService.GetJobsByIdsAsync(jobIds.ToArray()).GetAwaiter().GetResult();

            var jobQueues = this._jobQueueAppService.GetQueuesAsync(null).GetAwaiter().GetResult();
            var enqueuedJobs = from queue in jobQueues
                               where jobIds.Contains(queue.JobId) &&
                               queue.FetchedAt == null
                               select queue;
            var jobsFiltered = enqueuedJobs
               .Select(jq => jobDetails.FirstOrDefault(job => job.Id == jq.JobId));

            var joinedJobs = jobsFiltered
               .Where(job => job != null)
               .Select(job =>
               {
                   var state = job.StateHistory.LastOrDefault();
                   return new JobSummary
                   {
                       Id = job.Id.ToString(),
                       InvocationData = job.InvocationData,
                       Arguments = job.Arguments,
                       CreatedAt = job.CreatedAt,
                       ExpireAt = job.ExpireAt,
                       FetchedAt = null,
                       StateName = job.StateName,
                       StateReason = state?.Reason,
                       StateData = state?.Data
                   };
               })
               .ToList();
            return DeserializeJobs(
                joinedJobs,
                (sqlJob, job, stateData) => new EnqueuedJobDto
                {
                    Job = job,
                    State = sqlJob.StateName,
                    EnqueuedAt = sqlJob.StateName == EnqueuedState.StateName
                        ? Hangfire.Common.JobHelper.DeserializeNullableDateTime(stateData["EnqueuedAt"])
                        : null
                });
        }

        private long GetNumberOfJobsByStateName(string stateName)
        {
            var all_job = this._jobAppService.GetJobsAsync().GetAwaiter().GetResult();
            var count = all_job.Count(u => u.StateName == stateName);
            return count;
        }

        private static JobList<TDto> DeserializeJobs<TDto>(ICollection<JobSummary> jobs,
            Func<JobSummary, Job, Dictionary<string, string>, TDto> selector)
        {
            var result = new List<KeyValuePair<string, TDto>>(jobs.Count);

            foreach (var job in jobs)
            {
                var stateData = job.StateData;
                var dto = selector(job, DeserializeJob(job.InvocationData, job.Arguments), stateData);
                result.Add(new KeyValuePair<string, TDto>(job.Id, dto));
            }

            return new JobList<TDto>(result);
        }

        private static Job DeserializeJob(string invocationData, string arguments)
        {
            var data = Hangfire.Common.JobHelper.FromJson<InvocationData>(invocationData);
            data.Arguments = arguments;

            try
            {
                return data.Deserialize();
            }
            catch (JobLoadException)
            {
                return null;
            }
        }
    }
}
