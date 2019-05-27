using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire.Common;
using Hangfire.States;
using Hangfire.Storage;
using Hangfire.Storage.Monitoring;
using HangFireStorageService.Dto;
using HangFireStorageService.Servces;

namespace HangFireStorageService.Internal
{
    public class ServiceFabricMonitoringApi : IMonitoringApi
    {
        private readonly ServiceFabricStorage _storage;
        private readonly IJobQueueAppService _jobQueueAppService;
        private readonly IJobAppService _jobAppService;
        private readonly IJobStateDataAppService _jobStateDataAppService;
        private readonly IServerAppService _serverAppService;
        private readonly ICounterAppService _counterAppService;
        private readonly IAggregatedCounterAppService _aggregatedCounterAppService;
        private readonly IJobSetAppService _jobSetAppService;

        public ServiceFabricMonitoringApi(ServiceFabricStorage storage,
            IJobQueueAppService jobQueueAppService,
            IJobAppService jobAppService,
            IJobStateDataAppService jobStateDataAppService,
            IServerAppService serverAppService,
            ICounterAppService counterAppService,
            IAggregatedCounterAppService aggregatedCounterAppService,
            IJobSetAppService jobSetAppService
            )
        {
            this._storage = storage;
            this._jobQueueAppService = jobQueueAppService;
            this._serverAppService = serverAppService;
            this._counterAppService = counterAppService;
            this._aggregatedCounterAppService = aggregatedCounterAppService;
            this._jobSetAppService = jobSetAppService;
        }

        public IList<QueueWithTopEnqueuedJobsDto> Queues()
        {

            var job_queueus = this._jobQueueAppService.GetAllJobQueueusAsync().GetAwaiter().GetResult();
            var result = new List<QueueWithTopEnqueuedJobsDto>(job_queueus.Count);
            //需要更具queue进行一次分组
            var groupd_queues = job_queueus.GroupBy(u => u.Queue);
            foreach (var queue in groupd_queues)
            {
                var enqueueJobIds = queue
                    .Where(u => u.FetchedAt == null)
                    .OrderBy(u => u.JobId)
                    .Take(5)
                    .Select(u => u.JobId)
                    .ToList();
                var array = queue.ToArray(); ;
                var enqueuedCount = array.Aggregate(0, (a, netx) =>
                {
                    if (netx.FetchedAt == null) return ++a;
                    return 0;
                });
                var fetchedCount = array.Aggregate(0, (a, next) =>
                {
                    if (next.FetchedAt != null) return ++a;
                    return 0;
                });
                //var firstJobs = ,call EnquequeJObs
                result.Add(new QueueWithTopEnqueuedJobsDto()
                {
                    Name = queue.Key,
                    Length = enqueuedCount,
                    Fetched = fetchedCount,
                    FirstJobs = null
                });
            }
            return result;
        }

        private void EnqueueJobs(long[] jobIds)
        {
            var all_jobs = this._jobAppService.GetAllJobsAsync().GetAwaiter().GetResult();
            var all_state = this._jobStateDataAppService.GetAllStateAsync().GetAwaiter().GetResult();
            var job_states = from job in all_jobs
                             from state in all_state
                             where jobIds.Contains(job.Id) &&
                             job.Id == state.JobId &&
                             state.Id == job.StateId
                             select new
                             {
                                 job,
                                 state
                             };
            var jobs = job_states.ToDictionary(u => u.job.Id, u => u);
            var sortedJobs = jobIds.Select(jobId => jobs.ContainsKey(jobId) ? jobs[jobId] : null).ToList();

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
            var job = this._jobAppService.GetJobAsync(long.Parse(jobId)).GetAwaiter().GetResult();
            var jobStates = this._jobStateDataAppService.GetStates(long.Parse(jobId)).GetAwaiter().GetResult();
            var history = jobStates.Select(u => new StateHistoryDto()
            {
                StateName = u.Name,
                CreatedAt = u.CreatedAt,
                Reason = u.Reason,
                Data = new Dictionary<string, string>(SerializationHelper.Deserialize<Dictionary<string, string>>(u.Data))
            }).ToList();
            var jobData = InvocationData.DeserializePayload(job.InvocationData);
            if (!string.IsNullOrEmpty(job.Arguments))
                jobData.Arguments = job.Arguments;
            var jobEntity = jobData.DeserializeJob();
            var result = new JobDetailsDto()
            {
                CreatedAt = job.CreatedAt,
                ExpireAt = job.ExpireAt,
                Properties = job.Parameters,
                History = history,
                Job = jobEntity
            };
            return result;
        }

        public StatisticsDto GetStatistics()
        {
            var statisticsDto = new StatisticsDto();
            var all_job = this._jobAppService.GetAllJobsAsync().GetAwaiter().GetResult();
            var all_counter = this._counterAppService.GetAllCounterAsync().GetAwaiter().GetResult();
            var all_aggregatedCounter = this._aggregatedCounterAppService.GetAllCounterAsync().GetAwaiter().GetResult();
            var all_server = this._serverAppService.GetAllServerAsync().GetAwaiter().GetResult();
            var all_sets = this._jobSetAppService.GetAllSetsAsync().GetAwaiter().GetResult();
            all_job.ForEach(u =>
            {
                if (u.StateName == "Enqueued")
                    statisticsDto.Enqueued++;
                else if (u.StateName == "Failed")
                    statisticsDto.Failed++;
                else if (u.StateName == "Processing")
                    statisticsDto.Processing++;
                else if (u.StateName == "Scheduled")
                    statisticsDto.Scheduled++;
            });
            all_counter.ForEach(u =>
            {
                if (u.Key == "stats:succeeded")
                    statisticsDto.Succeeded++;
                if (u.Key == "stats:deleted")
                    statisticsDto.Deleted++;
            });
            all_aggregatedCounter.ForEach(u =>
            {
                if (u.Key == "stats:succeeded")
                    statisticsDto.Succeeded++;
                if (u.Key == "stats:deleted")
                    statisticsDto.Deleted++;
            });
            statisticsDto.Servers = all_server.Count();
            statisticsDto.Recurring = all_sets.Count(u => u.Key == "recurring-jobs");
            return statisticsDto;
        }

        public JobList<EnqueuedJobDto> EnqueuedJobs(string queue, int @from, int perPage)
        {
            var job_queueus = this._jobQueueAppService.GetAllJobQueueusAsync().GetAwaiter().GetResult();
            var enqueueJobIds = job_queueus
                  .Where(u => u.FetchedAt == null && u.Queue == queue)
                  .OrderBy(u => u.JobId)
                  .Take(5)
                  .Select(u => u.JobId)
                  .ToList();
            this.EnqueueJobs(enqueueJobIds.ToArray());
            return null; //TODO:这里是一个重构的点
        }

        public JobList<FetchedJobDto> FetchedJobs(string queue, int @from, int perPage)
        {
            throw new NotImplementedException();
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
                    EnqueueAt = JobHelper.DeserializeNullableDateTime(stateData["EnqueueAt"]) ?? DateTime.MinValue,
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
            var count = this._jobAppService.GetNumberbyStateName(ScheduledState.StateName).GetAwaiter().GetResult();
            return count;
        }

        public long EnqueuedCount(string queue)
        {
            throw new NotImplementedException();
        }

        public long FetchedCount(string queue)
        {
            throw new NotImplementedException();
        }

        public long FailedCount()
        {
            var count = this._jobAppService.GetNumberbyStateName(FailedState.StateName).GetAwaiter().GetResult();
            return count;
        }

        public long ProcessingCount()
        {
            var count = this._jobAppService.GetNumberbyStateName(ProcessingState.StateName).GetAwaiter().GetResult();
            return count;
        }

        public long SucceededListCount()
        {
            var count = this._jobAppService.GetNumberbyStateName(SucceededState.StateName).GetAwaiter().GetResult();
            return count;
        }

        public long DeletedListCount()
        {
            var count = this._jobAppService.GetNumberbyStateName(DeletedState.StateName).GetAwaiter().GetResult();
            return count;
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
        Func<JobDto, Job, Dictionary<string, string>, TDto> selector)
        {
            var all_jobs = this._jobAppService.GetAllJobsAsync().GetAwaiter().GetResult();
            var all_states = this._jobStateDataAppService.GetAllStateAsync().GetAwaiter().GetResult();
            //var jobs = all_jobs.Where(u => u.StateName == stateName).OrderByDescending(u => u.Id).Skip(from).Take(count).ToList();
            //var jobIds = jobs.Select(u => u.Id).ToList();
            //var stateIds = jobs.Select(u => u.StateId).ToList();
            //var states=all_states.Where(u=>jobIds.Contains(u.JobId)&&stateIds.Contains(u.Id)

            var query = from job in all_jobs
                        from state in all_states
                        where job.StateName == stateName &&
                        job.Id == state.JobId &&
                        job.StateId == state.Id
                        select new { job, state };
            var jobs = query.OrderByDescending(u => u.job.Id).Skip(from).Take(count).ToList();
            var result = new List<KeyValuePair<string, TDto>>(jobs.Count);
            foreach (var job in jobs)
            {
                var dto = default(TDto);

                if (job.job.InvocationData != null)
                {
                    var deserializedData = SerializationHelper.Deserialize<Dictionary<string, string>>(job.state.Data);
                    var stateData = deserializedData != null
                        ? new Dictionary<string, string>(deserializedData, StringComparer.OrdinalIgnoreCase)
                        : null;
                    var data = InvocationData.DeserializePayload(job.job.InvocationData);
                    if (!string.IsNullOrEmpty(job.job.Arguments))
                    {
                        data.Arguments = job.job.Arguments;
                    }
                    var jobEntity = data.DeserializeJob();
                    dto = selector(job.job, jobEntity, stateData);
                }

                result.Add(new KeyValuePair<string, TDto>(
                    job.job.Id.ToString(), dto));
            }
            return new JobList<TDto>(result);
        }
    }
}
