using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.ServiceFabric.Entities
{
    public class EnqueuedAndFetchedCountDto
    {
        public int? EnqueuedCount { get; set; }

        public int? FetchedCount { get; set; }
    }
}
