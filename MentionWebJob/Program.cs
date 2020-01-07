using System;
using System.Net;
using Microsoft.Azure.WebJobs;

namespace MentionWebJob
{
    class Program
    {
        static void Main()
        {
            var config = new JobHostConfiguration();

            if (config.IsDevelopment)
            {
                config.UseDevelopmentSettings();
            }

            ServicePointManager.DefaultConnectionLimit = Int32.MaxValue;
            var host = new JobHost(config);
            host.Call(typeof(Functions).GetMethod("ProcessRetweets"));
        }
    }
}
