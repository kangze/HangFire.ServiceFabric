﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SampleAspNetCore.Pages
{
    public class IndexModel : PageModel
    {
        public void OnGet()
        {
            //基于队列的任务处理
            var jobId = BackgroundJob.Enqueue(() => Console.WriteLine("Fire-and-forget!"));

            ////延迟任务执行
            //var jobId2 = BackgroundJob.Schedule(() => System.Diagnostics.Debug.WriteLine("Delayed!"), TimeSpan.FromMinutes(2));

            ////定时任务执行
            //RecurringJob.AddOrUpdate(() => System.Diagnostics.Debug.WriteLine("Recurring!"), Cron.Minutely);

            ////延续性任务执行,延续job1
            //BackgroundJob.ContinueJobWith(jobId, () => System.Diagnostics.Debug.WriteLine("Continuation!"));
        }
    }
}
