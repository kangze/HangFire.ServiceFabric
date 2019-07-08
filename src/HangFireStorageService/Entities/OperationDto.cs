using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HangFireStorageService.Dto
{
    public class OperationDto
    {
        public OperationEnum OperationType { get; set; }

        public object Argument { get; set; }
    }
}
