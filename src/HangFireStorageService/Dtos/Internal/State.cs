using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.ServiceFabric.Dtos.Internal
{
    internal class State
    {
        public const string Succeeded = "succeeded";

        public const string Processing = "processing";

        public const string Failed = "failed";

        public const string Deleted = "deleted";
    }
}
