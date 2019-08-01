using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.ServiceFabric.Dtos
{
    public class ListDto
    {
        public string Id { get; set; }

        public DateTime? ExpireAt { get; set; }


        public string Item { get; set; }

        
        public string Value { get; set; }
    }
}
