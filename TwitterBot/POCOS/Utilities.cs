using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using TwitterBot.Models;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using System;
using Microsoft.ApplicationInsights.DataContracts;

namespace TwitterBot.POCOS
{
    public static class Utilities
    {

        
        static readonly string instrumentationKey = Environment.GetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY");
        

        public static bool IsAdmin(IEnumerable<Claim> claims, TwitterBotContext databaseContext)
        {
            var preferredUsername = UsernameFromClaims(claims);
            return databaseContext.AdminManagers
                                  .FirstOrDefault(
                admins => admins.User == preferredUsername) != null;
        }

        static string UsernameFromClaims(IEnumerable<Claim> claims) =>
            claims.FirstOrDefault(claim => claim.Type == "preferred_username")
                  ?.Value ?? "";

        public static void Log(string api, Dictionary<string,string> properties)
        {
            if (!(String.IsNullOrEmpty(Utilities.instrumentationKey)))
            {
                Console.WriteLine("Log with instructionKey");
                TelemetryConfiguration configuration = TelemetryConfiguration.CreateDefault();
                configuration.InstrumentationKey = instrumentationKey;
                var telemetryClient = new TelemetryClient(configuration);

                var messageTitle = ($"{api}");

                telemetryClient.TrackTrace(messageTitle, SeverityLevel.Information, properties);
            }
        }

        public static void Log(string api, string message)
        {
            if (!(String.IsNullOrEmpty(Utilities.instrumentationKey)))
            {
                Console.WriteLine("Log with instructionKey");
                TelemetryConfiguration configuration = TelemetryConfiguration.CreateDefault();
                configuration.InstrumentationKey = instrumentationKey;
                var telemetryClient = new TelemetryClient(configuration);

                var messageTitle = ($"{api} - {message}");

                telemetryClient.TrackTrace(messageTitle, SeverityLevel.Information);
            }
        }
    }
}