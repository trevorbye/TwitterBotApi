using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;

namespace TwitterWebJob
{
    public class Functions
    {
        [NoAutomaticTrigger]
        public static void ProcessTweets(TextWriter log)
        {
            log.Write("service ran at" + DateTime.UtcNow.ToString());
        }
    }
}
