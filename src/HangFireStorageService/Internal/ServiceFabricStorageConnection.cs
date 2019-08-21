using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hangfire.Annotations;
using Hangfire.Common;
using Hangfire.Server;
using Hangfire.ServiceFabric.Dtos;
using Hangfire.ServiceFabric.Internal;
using Hangfire.ServiceFabric.Servces;
using Hangfire.Storage;
using HangFireStorageService.Dto;

namespace Hangfire.ServiceFabric.Internal
{
    internal class ServiceFabricStorageConnection : JobStorageConnection
    {
        private readonly IServiceFabriceStorageServices _services;

        public static AutoResetEvent AutoResetNewEvent = new AutoResetEvent(true);

        public ServiceFabricStorageConnection(IServiceFabriceStorageServices servies)
        {
            this._services = servies;
        }

        public ServiceFabricWriteOnlyTransaction CreateTransaction()
        {
            return new ServiceFabricWriteOnlyTransaction(this._services);
        }

        public override void Dispose() { }

        public override IWriteOnlyTransaction CreateWriteTransaction()
        {
            return new ServiceFabricWriteOnlyTransaction(this._services);
        }

        public override IDisposable AcquireDistributedLock(string resource, TimeSpan timeout)
        {
            var distributedLock = new ServiceFabricDistributedLock(resource, timeout, this._services.ResourceLockAppService);
            return distributedLock;
        }

        public override string CreateExpiredJob(Job job, IDictionary<string, string> parameters, DateTime createdAt, TimeSpan expireIn)
        {
            using (var transaction = CreateTransaction())
            {
                var jobId = transaction.CreateExpiredJob(job, parameters, createdAt, expireIn);
                transaction.Commit();
                return jobId;
            }

        }

        public override IFetchedJob FetchNextJob(string[] queues, CancellationToken cancellationToken)
        {
            do
            {
                AutoResetNewEvent.WaitOne();
                foreach (var queue in queues)
                {
                    var fetchedJob = this._services.JobQueueAppService.GetFetchedJobAsync(queue).GetAwaiter().GetResult();
                    if (fetchedJob == null)
                        continue;
                    fetchedJob.FetchedAt = DateTime.Now;
                    this._services.JobQueueAppService.UpdateQueueAsync(fetchedJob);
                    return new ServiceFabricFetchedJob(fetchedJob.Id, fetchedJob.JobId, queue, this._services.JobQueueAppService);
                }
            } while (true);
        }

        public override void SetJobParameter(string id, string name, string value)
        {
            using (var transaction = CreateTransaction())
            {
                transaction.SetJobParameter(id, name, value).GetAwaiter().GetResult();
                transaction.Commit();
            }
        }

        public override string GetJobParameter(string id, string name)
        {
            var job = this._services.JobAppService.GetJobAsync(id).GetAwaiter().GetResult();
            if (job == null)
                throw new Exception("没有找到任务");
            var parameter = job.Parameters.FirstOrDefault(u => u.Key == name);
            return parameter.Value;

        }

        public override JobData GetJobData(string jobId)
        {
            if (string.IsNullOrEmpty(jobId))
                return null;
            var job = this._services.JobAppService.GetJobAsync(jobId).GetAwaiter().GetResult();
            if (job == null)
                return null;
            var invocationData = InvocationData.DeserializePayload(job.InvocationData);
            var jobData = new JobData()
            {
                CreatedAt = job.CreatedAt,
                LoadException = null,
                Job = invocationData.DeserializeJob(),
                State = job.StateName
            };
            return jobData;
        }

        public override StateData GetStateData(string jobId)
        {
            if (jobId == null)
            {
                throw new ArgumentNullException(nameof(jobId));
            }

            var job = this._services.JobAppService.GetJobAsync(jobId).GetAwaiter().GetResult();

            if (job == null)
            {
                return null;
            }

            var state = job.StateHistory.LastOrDefault();

            if (state == null)
            {
                return null;
            }

            return new StateData
            {
                Name = state.Name,
                Reason = state.Reason,
                Data = state.Data
            };
        }

        public override void AnnounceServer(string serverId, ServerContext context)
        {
            var serverData = new ServerData()
            {
                Queues = context.Queues,
                WorkCount = context.WorkerCount,
                StartedAt = DateTime.UtcNow,
            };
            this._services.ServerAppService.AddOrUpdateAsync(serverId, SerializationHelper.Serialize(serverData), DateTime.UtcNow).GetAwaiter().GetResult();

        }

        public override void RemoveServer(string serverId)
        {
            this._services.ServerAppService.RemoveServer(serverId);
        }

        public override void Heartbeat(string serverId)
        {
            var server = this._services.ServerAppService.GetServerAsync(serverId).GetAwaiter().GetResult();
            if (server == null)
                throw new Exception("Has not found that server,serverId:" + serverId);
            this._services.ServerAppService.AddOrUpdateAsync(serverId, server.Data, DateTime.UtcNow);
        }

        public override int RemoveTimedOutServers(TimeSpan timeOut)
        {
            if (timeOut.Duration() != timeOut)
            {
                throw new ArgumentException("The `timeOut` value must be positive.", nameof(timeOut));
            }
            int count = 0;
            var serverDtos = this._services.ServerAppService.GetAllServerAsync().GetAwaiter().GetResult();
            foreach (var server in serverDtos)
            {
                if (server.LastHeartbeat < DateTime.UtcNow.Add(timeOut.Negate()))
                {
                    this._services.ServerAppService.RemoveServer(server.ServerId).GetAwaiter().GetResult();
                    count++;
                }
            }
            return count;
        }

        public override HashSet<string> GetAllItemsFromSet(string key)
        {
            var all_sets = this._services.JobSetAppService.GetSetsAsync().GetAwaiter().GetResult();
            return all_sets.Where(u => u.Key == key).Select(u => u.Value).ToHashSet();
        }

        public override string GetFirstByLowestScoreFromSet(string key, double fromScore, double toScore)
        {
            return GetFirstByLowestScoreFromSet(key, fromScore, toScore, 1).FirstOrDefault();
        }

        public override List<string> GetFirstByLowestScoreFromSet(string key, double fromScore, double toScore, int count)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (toScore < fromScore) throw new ArgumentException("The `toScore` value must be higher or equal to the `fromScore` value.", nameof(toScore));

            var all_sets = this._services.JobSetAppService.GetSetsAsync().GetAwaiter().GetResult();
            var sets = all_sets.Where(u => u.Score >= fromScore && u.Score <= toScore).OrderBy(u => u.Score).Take(count);
            return sets.Select(u => u.Value).ToList();
        }

        public override void SetRangeInHash(string key, IEnumerable<KeyValuePair<string, string>> keyValuePairs)
        {
            using (var transaction = CreateTransaction())
            {
                transaction.SetRangeInHash(key, keyValuePairs);
                transaction.Commit();
            }
        }

        public override Dictionary<string, string> GetAllEntriesFromHash(string key)
        {
            var hash = this._services.HashAppService.GetHashDtoAsync(key).GetAwaiter().GetResult();

            return hash == null ? new Dictionary<string, string>() : hash.Fields;
        }

        public override long GetCounter(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            var counter = this._services.CounterAppService.GetCounterAsync(key).GetAwaiter().GetResult();

            return counter?.Value ?? 0;
        }

        public override long GetSetCount([NotNull] string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            var sets = this._services.JobSetAppService.GetSetsAsync().GetAwaiter().GetResult();
            var count = sets.Where(u => u.Key.Contains(key)).Count();
            return count;
        }

        public override List<string> GetRangeFromList([NotNull] string key, int startingFrom, int endingAt)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            var listDtos = this._services.ListAppService.GetListDtoAsync(key).GetAwaiter().GetResult();
            var listValues = listDtos.Where(u => u.Item.Contains(key))
                .OrderBy(u => u.Id)
                .Skip(startingFrom)
                .Take(endingAt - startingFrom + 1)
                .Select(u => u.Value)
                .ToList();
            return listValues;
        }

        public override long GetHashCount([NotNull] string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            var hash = this._services.HashAppService.GetHashDtoAsync(key).GetAwaiter().GetResult();

            return hash?.Fields.Count ?? 0;
        }

        public override List<string> GetAllItemsFromList([NotNull] string key)
        {
            var list = this._services.ListAppService.GetListDtoAsync(key).GetAwaiter().GetResult();
            return list.Select(u => u.Value).ToList();
        }

        public override TimeSpan GetHashTtl([NotNull] string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            var hashes = this._services.HashAppService.GetAllHashAsync().GetAwaiter().GetResult();
            var result = hashes.Where(u => u.Key == key)
                .OrderBy(u => u.ExpireAt)
                .Select(u => u.ExpireAt)
                .FirstOrDefault();

            return result.HasValue ? result.Value - DateTime.UtcNow : TimeSpan.FromSeconds(-1);
        }

        public override long GetListCount([NotNull] string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            var list = this._services.ListAppService.GetListDtoAsync(key).GetAwaiter().GetResult();
            return list.Count;
        }

        public override List<string> GetRangeFromSet([NotNull] string key, int startingFrom, int endingAt)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            var setDtos = this._services.JobSetAppService.GetSetsAsync().GetAwaiter().GetResult();
            var setValues = setDtos.Where(u => u.Key.Contains(key))
                .OrderBy(u => u.Id)
                .Skip(startingFrom)
                .Take(endingAt - startingFrom + 1)
                .Select(u => u.Value)
                .ToList();
            return setValues;
        }

        public override TimeSpan GetListTtl([NotNull] string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            var list = this._services.ListAppService.GetListDtoAsync(key).GetAwaiter().GetResult();
            var expireAt = list.OrderBy(u => u.ExpireAt).Select(u => u.ExpireAt).FirstOrDefault();

            return expireAt.HasValue ? expireAt.Value - DateTime.UtcNow : TimeSpan.FromSeconds(-1);
        }

        public override TimeSpan GetSetTtl([NotNull] string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            var setDtos = this._services.JobSetAppService.GetSetsAsync().GetAwaiter().GetResult();
            var expireAts = setDtos
                .Where(u => u.Key.Contains(key) && u.ExpireAt != null)
                .Select(u => u.ExpireAt)
                .ToList();
            return expireAts.Any() ? expireAts.Min() - DateTime.UtcNow : TimeSpan.FromSeconds(-1);

        }

        public override string GetValueFromHash([NotNull] string key, [NotNull] string name)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            var hashDto = this._services.HashAppService.GetHashDtoAsync(key).GetAwaiter().GetResult();
            if (hashDto != null && hashDto.Fields.ContainsKey(name))
                return hashDto.Fields[name];
            return null;

        }
    }
}
