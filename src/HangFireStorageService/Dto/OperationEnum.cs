using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HangFireStorageService.Dto.Internal;

namespace HangFireStorageService.Dto
{
    public enum OperationEnum
    {
        [Description("Expire Time from DateTime.Now")]
        Expire = 0,

        [Description("Persita a Job,Tip:set Exprit to Null")]
        Persist = 1,

        [Description("Update job sate point to")]
        SetJobState = 2,

        [Description("Add job state infomation")]
        //[ArgumentType(typeof(AddJobStateArgument))]
        AddJobState = 3,

        [Description("Add a job to queue")]
        AddToQueque = 4,

        [Description("IncrementCounter")]
        Increment = 5,

        [Description("IncreatementCounter with Expire time")]
        IncrementWithExpire = 6,

        [Description("DeIncrementCounter")]
        DeIncrement = 7,

        [Description("DeIncreatementCounter with Expire time")]
        DeIncrementWithExpire = 8,

        [Description("Add job infomation(JobId and type) to Set")]
        AddToSet = 9,

        [Description("Add job infomation(JobId and type) to Set")]
        AddToSetWithScore = 10,

        [Description("Remove a job infomation(JobId and type) from Set")]
        RemoveFromSet = 11,

        InsertToList = 12,

        RemoveFromList = 13,

        TrimList = 14,

        SetRangeInHash = 15,

        RemoveHash = 16
    }
}
