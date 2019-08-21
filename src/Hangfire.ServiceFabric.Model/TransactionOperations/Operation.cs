using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.ServiceFabric.Model.TransactionOperations
{
    public abstract class Operation
    {
        public abstract OperationType OperationType { get; }

        public abstract T GetArguments<T>(object obj);
    }

    public enum OperationType
    {
        ExpireJob = 0,
        CreateExpiredJob = 1,
        SetJobParameter = 2,
        PersistJob = 3,
        SetJobState = 4,
        AddJobState = 5,
        AddToQueue = 6,
        IncrementCounter = 7,
        DecrementCounter = 8,
        AddToSet = 9,
        RemoveFromSet = 10,
        InsertToList = 11,
        RemoveFromList = 12,
        TrimList = 13,
        SetRangeInHash = 14,
        RemoveHash = 15,
    }
}
