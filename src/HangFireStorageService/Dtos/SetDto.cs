using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.ServiceFabric.Dtos
{
    public class SetDto
    {
        public string Id { get; set; }

        public string Key { get; set; }

        public double Score { get; set; }

        public string Value { get; set; }

        public DateTime ExpireAt { get; set; }
    }
}
