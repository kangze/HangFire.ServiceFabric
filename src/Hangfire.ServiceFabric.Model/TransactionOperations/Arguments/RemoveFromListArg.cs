﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.ServiceFabric.Model.TransactionOperations.Arguments
{
    public class RemoveFromListArg
    {
        public string Key { get; set; }

        public string Value { get; set; }
    }
}
