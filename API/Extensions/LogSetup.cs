using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.HttpLogging;
using Serilog;
using System.Diagnostics.CodeAnalysis;

namespace API.Extensions;

[ExcludeFromCodeCoverage]
public static class LogSetup
{
    public static void UseSerilog(Microsoft.AspNetCore.Builder.WebApplicationBuilder builder )
    {
        //if (builder.Environment.EnvironmentName.Equals("Development", StringComparison.Ordinal))
        //{
        //    var seqUrl = builder.Configuration.GetRequiredSection("Seq").Get<Seq>()?.Url;

        //    builder.Host.UseSerilog(new LoggerConfiguration()
        //        .WriteTo.Console()
        //        .WriteTo.Debug()
        //        .WriteTo.Seq(serverUrl: seqUrl!)
        //        .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", Serilog.Events.LogEventLevel.Warning)
        //        .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
        //        .Filter.ByExcluding(c =>
        //            c.Properties.Any(p => p.Value.ToString().Contains("swagger") ||
        //                                  p.Value.ToString().Contains("health")))
        //        .Filter.ByExcluding(c => c.MessageTemplate.Text.Contains("health"))
        //        .ReadFrom.Configuration(builder.Configuration)
        //        .CreateLogger());
        //}
        //else
        //{
            var appInsightInstrumentationKey = builder.Configuration.GetRequiredSection("ApplicationInsights:InstrumentationKey").Value;

            builder.Host.UseSerilog(new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.Debug()
                .WriteTo.ApplicationInsights(new TelemetryConfiguration { InstrumentationKey = appInsightInstrumentationKey }, TelemetryConverter.Traces)
                .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
                .Filter.ByExcluding(c =>
                    c.Properties.Any(p => p.Value.ToString().Contains("swagger") ||
                                          p.Value.ToString().Contains("health")))
                .Filter.ByExcluding(c => c.MessageTemplate.Text.Contains("health"))
                .ReadFrom.Configuration(builder.Configuration)
                .CreateLogger());
        //}
    }

    public static void AddHttpLogging(this IServiceCollection services)
    {
        services.AddHttpLogging(logging =>
        {
            // Customize HTTP logging here to log Request and response on each call to the API https://medium.com/@luisalexandre.rodrigues/logging-http-request-and-response-in-net-web-api-268135dcb27b
            logging.LoggingFields = HttpLoggingFields.All;
            logging.RequestHeaders.Add("Authorization");
            logging.RequestHeaders.Add("sec-ch-ua");
            logging.RequestHeaders.Add("sec-ch-ua-mobile");
            logging.RequestHeaders.Add("sec-ch-ua-platform");
            logging.RequestHeaders.Add("sec-fetch-site");
            logging.RequestHeaders.Add("sec-fetch-mode");
            logging.RequestHeaders.Add("sec-fetch-dest");
            logging.RequestBodyLogLimit = 4096;
            logging.ResponseBodyLogLimit = 4096;
        });
    }
}