using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HangFireStorageService.Dto
{
    public class HashDto
    {
        public string Id { get; set; }

        public string Key { get; set; }

        public Dictionary<string, string> Fields { get; set; }

        public DateTime? ExpireAt { get; set; }
    }
}
