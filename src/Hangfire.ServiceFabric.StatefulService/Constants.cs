using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.ServiceFabric.StatefulService
{
    public static class Constants
    {
        public const string ListenerNames_JobAppService = "JobAppService";
        public const string ListenerNames_JobQueueAppService = "JobQueueAppService";
        public const string ListenerNames_JobStateDataAppService = "JobStateDataAppService";
        public const string ListenerNames_ServerAppService = "ServerAppService";
        public const string ListenerNames_CounterAppService = "CounterAppService";
        public const string ListenerNames_AggregatedCounterAppService = "AggregatedCounterAppService";
        public const string ListenerNames_JobSetAppService = "JobSetAppService";
        public const string ListenerNames_JobDataService = "JobDataService";
        public const string ListenerNames_HashAppService = "HashAppService";
        public const string ListenerNames_jobListAppSerivce = "JobListAppService";
        public const string ListenerNames_ResourceLockAppService = "ResourceLockAppService";
    }
}
