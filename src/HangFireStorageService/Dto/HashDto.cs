using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HangFireStorageService.Dto
{
    public class HashDto
    {

        public string Key { get; set; }

        public string Field { get; set; }

        public string Value { get; set; }

        public DateTime ExpireAt { get; set; }
    }
}
