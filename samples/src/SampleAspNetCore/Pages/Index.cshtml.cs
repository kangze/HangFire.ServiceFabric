using System;
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
            var jobId = BackgroundJob.Enqueue(() =>
               Test()
                );

            ////延迟任务执行
            var jobId2 = BackgroundJob.Schedule(() => Console.WriteLine("Delayed!"), TimeSpan.FromMinutes(2));

            ////定时任务执行
            RecurringJob.AddOrUpdate(() => Console.WriteLine("Recurring!"), Cron.Minutely);

            ////延续性任务执行,延续job1
            var s = BackgroundJob.ContinueJobWith(jobId, () => Console.WriteLine("Continuation!"));
        }

        public void Test()
        {
            //syste
            Console.WriteLine("ok enqueued Jobs!!!");
        }
    }
}
