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
            RecurringJob.AddOrUpdate(() => Console.WriteLine("Transparent!"), Cron.Daily);
        }
    }
}
