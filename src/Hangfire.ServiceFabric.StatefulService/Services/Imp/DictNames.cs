using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.ServiceFabric.StatefulService.Services.Imp
{
    public class DictNames
    {
        public string JobDictName { get; }

        public string ServerDictName { get; }

        public string SetDictName { get; }

        public string HashDictName { get; }

        public string JobQueueDictName { get; }

        public string AggregatdcounterDictName { get; }

        public string CounterDictName { get; }

        public string ListDictName { get; }

        public string LockDictName { get; }


        public DictNames(string prefix)
        {
            this.JobDictName = string.Format(Consts.JOB_DICT, prefix);
            this.ServerDictName = string.Format(Consts.SERVER_DICT, prefix);
            this.SetDictName = string.Format(Consts.SET_DICT, prefix);
            this.HashDictName = string.Format(Consts.HASH_DICT, prefix);
            this.JobQueueDictName = string.Format(Consts.JOBQUEUE_DICT, prefix);
            this.AggregatdcounterDictName = string.Format(Consts.AGGREGATEDCOUNTER, prefix);
            this.CounterDictName = string.Format(Consts.COUNTER, prefix);
            this.ListDictName = string.Format(Consts.LIST_DICT, prefix);
            this.LockDictName = string.Format(Consts.LOCK_DICT, prefix);
        }
    }

    public static class Consts
    {
        public static string JOB_DICT = "{0}_JOB_DICT";
        public static string STATE_DICT = "{0}_STATE_DICT";
        public static string SERVER_DICT = "{0}_SERVER_DICT";
        public static string SET_DICT = "{0}_SET_DICT";
        public static string HASH_DICT = "{0}_HASH_DICT";
        public static string JOBQUEUE_DICT = "{0}_JOBQUEUE_DICT";
        public static string AGGREGATEDCOUNTER = "{0}_AGGREGATEDCOUNTER_DICT";
        public static string COUNTER = "{0}_COUNTER_DICT";
        public static string LIST_DICT = "{0}_LIST_DICT";
        public static string LOCK_DICT = "{0}_LOCK_DICT";
    }
}
