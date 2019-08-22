using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Hangfire.ServiceFabric.Model.TransactionOperations
{
    [DataContract]
    public class Operation
    {
        [DataMember]
        public OperationType OperationType { get; set; }

        public T GetArguments<T>()
        where T : class
        {
            return JsonConvert.DeserializeObject<T>(this.Arg);
        }

        [DataMember]
        public string Arg { get; set; }
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
