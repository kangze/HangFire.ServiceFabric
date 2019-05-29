using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire.States;
using Hangfire.Storage;
using HangFireStorageService.Dto;
using HangFireStorageService.Servces;

namespace HangFireStorageService.Internal
{
    public class ServiceFabricWriteOnlyTransaction : JobStorageTransaction
    {
        private readonly List<OperationDto> _operations;
        private readonly IJobDataService _jobDataService;



        public ServiceFabricWriteOnlyTransaction(List<OperationDto> operations, IJobDataService jobDataService)
        {
            _operations = operations;
            _jobDataService = jobDataService;
        }

        public override void ExpireJob(string jobId, TimeSpan expireIn)
        {
            this._operations.Add(new OperationDto()
            {
                OperationType = OperationEnum.Expire,
                Argument = expireIn,
            });
        }

        public override void PersistJob(string jobId)
        {
            this._operations.Add(new OperationDto()
            {
                OperationType = OperationEnum.Persist,
                Argument = jobId,
            });
        }

        public override void SetJobState(string jobId, IState state)
        {
            this._operations.Add(new OperationDto()
            {
                OperationType = OperationEnum.SetJobState,
                Argument = state,
            });
        }

        public override void AddJobState(string jobId, IState state)
        {
            this._operations.Add(new OperationDto()
            {
                OperationType = OperationEnum.AddJobState,
                Argument = state,
            });
        }

        public override void AddToQueue(string queue, string jobId)
        {
            this._operations.Add(new OperationDto()
            {
                OperationType = OperationEnum.AddToQueque,
                Argument = new { queue, jobId },
            });
        }

        public override void IncrementCounter(string key)
        {
            this._operations.Add(new OperationDto()
            {
                OperationType = OperationEnum.Increment,
                Argument = key,
            });
        }

        public override void IncrementCounter(string key, TimeSpan expireIn)
        {
            this._operations.Add(new OperationDto()
            {
                OperationType = OperationEnum.IncrementWithExpire,
                Argument = new { key, expireIn },
            });
        }

        public override void DecrementCounter(string key)
        {
            this._operations.Add(new OperationDto()
            {
                OperationType = OperationEnum.DeIncrement,
                Argument = key,
            });
        }

        public override void DecrementCounter(string key, TimeSpan expireIn)
        {
            this._operations.Add(new OperationDto()
            {
                OperationType = OperationEnum.DeIncrementWithExpire,
                Argument = new { key, expireIn },
            });
        }

        public override void AddToSet(string key, string value)
        {
            throw new NotImplementedException();
        }

        public override void AddToSet(string key, string value, double score)
        {
            throw new NotImplementedException();
        }

        public override void RemoveFromSet(string key, string value)
        {
           throw new NotImplementedException();
        }

        public override void InsertToList(string key, string value)
        {
            throw new NotImplementedException();
        }

        public override void RemoveFromList(string key, string value)
        {
            throw new NotImplementedException();
        }

        public override void TrimList(string key, int keepStartingFrom, int keepEndingAt)
        {
            throw new NotImplementedException();
        }

        public override void SetRangeInHash(string key, IEnumerable<KeyValuePair<string, string>> keyValuePairs)
        {
            throw new NotImplementedException();
        }

        public override void RemoveHash(string key)
        {
            throw new NotImplementedException();
        }

        public override void Commit()
        {
            _jobDataService.UpdateJobAsync(this._operations).GetAwaiter().GetResult();
        }
    }
}
