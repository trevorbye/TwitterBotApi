using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;

namespace TwitterWebJob
{
    class Program
    {
        static async Task Main()
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
                await host.CallAsync(
                    typeof(Functions).GetMethod(
                        nameof(Functions.ProcessTweets)));
            }
        }
    }
}