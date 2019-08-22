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
using Hangfire.ServiceFabric.Model.TransactionOperations;
using Hangfire.ServiceFabric.Model.TransactionOperations.Arguments;
using Hangfire.States;
using Hangfire.Storage;
using HangFireStorageService.Dto;

namespace Hangfire.ServiceFabric.Internal
{
    public class ServiceFabricWriteOnlyTransaction : JobStorageTransaction
    {
        private readonly IServiceFabriceStorageServices _services;
        private readonly IList<Operation> _operations;
        public ServiceFabricWriteOnlyTransaction(IServiceFabriceStorageServices services)
        {
            this._services = services;
            this._operations = new List<Operation>();
        }

        public override void ExpireJob(string jobId, TimeSpan expireIn)
        {
            this._operations.Add(new Operation()
            {
                OperationType = OperationType.ExpireJob,
                Arg = new ExpireJobArg()
                {
                    JobId = jobId,
                    ExpireIn = expireIn,
                }
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
            this._operations.Add(new Operation()
            {
                OperationType = OperationType.CreateExpiredJob,
                Arg = new CreateExpiredJobArg()
                {
                    JobDto = jobDto
                }
            });
            return jobDto.Id;
        }

        public void SetJobParameter(string id, string name, string value)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            if (name == null)
                throw new ArgumentNullException(nameof(name));

            this._operations.Add(new Operation()
            {
                OperationType = OperationType.SetJobParameter,
                Arg = new SetJobParameterArg()
                {
                    JobId = id,
                    Name = name,
                    Value = value
                }
            });
        }

        public override void PersistJob(string jobId)
        {
            this._operations.Add(new Operation()
            {
                OperationType = OperationType.PersistJob,
                Arg = new PersistJobArg()
                {
                    JobId = jobId
                }
            });
        }

        public override void SetJobState(string jobId, IState state)
        {
            var stateDto = new StateDto
            {
                Name = state.Name,
                Reason = state.Reason,
                CreatedAt = DateTime.UtcNow,
                Data = state.SerializeData()
            };
            this._operations.Add(new Operation()
            {
                OperationType = OperationType.SetJobState,
                Arg = new SetJobStateArg()
                {
                    JobId = jobId,
                    StateDto = stateDto
                }
            });
        }

        public override void AddJobState(string jobId, IState state)
        {
            var stateDto = new StateDto
            {
                Name = state.Name,
                Reason = state.Reason,
                CreatedAt = DateTime.UtcNow,
                Data = state.SerializeData()
            };
            this._operations.Add(new Operation()
            {
                OperationType = OperationType.AddJobState,
                Arg = new SetJobStateArg()
                {
                    JobId = jobId,
                    StateDto = stateDto
                }
            });
        }

        public override void AddToQueue(string queue, string jobId)
        {
            this._operations.Add(new Operation()
            {
                OperationType = OperationType.AddToQueue,
                Arg = new AddToQueueArg()
                {
                    Id = Guid.NewGuid().ToString("N"),
                    JobId = jobId,
                    Queue = queue
                }
            });

            //   ServiceFabricStorageConnection.AutoResetNewEvent.Set();
        }

        public override void IncrementCounter(string key)
        {
            this._operations.Add(new Operation()
            {
                OperationType = OperationType.IncrementCounter,
                Arg = new IncrementCounterArg()
                {
                    Key = key,
                    Value = 1,
                }
            });
        }

        public override void IncrementCounter(string key, TimeSpan expireIn)
        {
            this._operations.Add(new Operation()
            {
                OperationType = OperationType.IncrementCounter,
                Arg = new IncrementCounterArg()
                {
                    Key = key,
                    Value = 1,
                    ExpireIn = expireIn
                }
            });
        }

        public override void DecrementCounter(string key)
        {
            this._operations.Add(new Operation()
            {
                OperationType = OperationType.IncrementCounter,
                Arg = new IncrementCounterArg()
                {
                    Key = key,
                    Value = -1,
                }
            });
        }

        public override void DecrementCounter(string key, TimeSpan expireIn)
        {
            this._operations.Add(new Operation()
            {
                OperationType = OperationType.IncrementCounter,
                Arg = new IncrementCounterArg()
                {
                    Key = key,
                    Value = -1,
                    ExpireIn = expireIn
                }
            });
        }

        public override void AddToSet(string key, string value)
        {
            AddToSet(key, value, 0.0);
        }

        public override void AddToSet(string key, string value, double score)
        {
            this._operations.Add(new Operation()
            {
                OperationType = OperationType.AddToSet,
                Arg = new AddToSetArg()
                {
                    Key = key,
                    Value = value,
                    Score = score
                }
            });
        }

        public override void RemoveFromSet(string key, string value)
        {
            this._operations.Add(new Operation()
            {
                OperationType = OperationType.RemoveFromSet,
                Arg = new RemoveFromSetArg()
                {
                    Key = key,
                    Value = value
                }
            });
        }

        public override void InsertToList(string key, string value)
        {
            this._operations.Add(new Operation()
            {
                OperationType = OperationType.InsertToList,
                Arg = new InsertToListArg()
                {
                    Key = key,
                    Value = value
                }
            });
        }

        public override void RemoveFromList(string key, string value)
        {
            this._operations.Add(new Operation()
            {
                OperationType = OperationType.RemoveFromList,
                Arg = new RemoveFromListArg()
                {
                    Key = key,
                    Value = value
                }
            });
        }

        public override void TrimList(string key, int keepStartingFrom, int keepEndingAt)
        {
            this._operations.Add(new Operation()
            {
                OperationType = OperationType.TrimList,
                Arg = new TrimListArg()
                {
                    Key = key,
                    KeepStartingFrom = keepStartingFrom,
                    KeepEndingAt = keepEndingAt
                }
            });
        }

        public override void SetRangeInHash(string key, IEnumerable<KeyValuePair<string, string>> keyValuePairs)
        {
            this._operations.Add(new Operation()
            {
                OperationType = OperationType.SetRangeInHash,
                Arg = new SetRangInHashArg()
                {
                    Key = key,
                    KeyValuePairs = keyValuePairs
                }
            });
        }

        public override void RemoveHash(string key)
        {
            this._operations.Add(new Operation()
            {
                OperationType = OperationType.RemoveHash,
                Arg = new RemoveHashArg() { Key = key }
            });
        }

        public override void Commit()
        {

        }
    }
}
