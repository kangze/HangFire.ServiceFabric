﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.ServiceFabric.Model.TransactionOperations.Arguments
{
    public class SetJobParameterArg: JobArg
    {
        public string Name { get; set; }

        public string Value { get; set; }
    }
}
