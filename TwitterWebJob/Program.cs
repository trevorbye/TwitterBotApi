using System.Net;
using Microsoft.Azure.WebJobs;

namespace TwitterWebJob
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

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.DefaultConnectionLimit = int.MaxValue;

            using (var host = new JobHost(config))
            {
                host.Call(typeof(Functions).GetMethod("ProcessTweets"));
            }
        }
    }
}