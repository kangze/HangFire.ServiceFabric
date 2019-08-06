﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SampleAspNetCore.Pages
{
    public class IndexModel : PageModel
    {
        public void OnGet()
        {
            Hangfire.RecurringJob.AddOrUpdate(() => Console.Write("1"), Hangfire.Cron.MinuteInterval(1));
        }
    }
}
