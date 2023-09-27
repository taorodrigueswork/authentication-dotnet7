using API.Configurations;
using Microsoft.ApplicationInsights.Extensibility;
using Serilog;

namespace API.Extensions;

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
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", Serilog.Events.LogEventLevel.Warning)
                .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
                .Filter.ByExcluding(c =>
                    c.Properties.Any(p => p.Value.ToString().Contains("swagger") ||
                                          p.Value.ToString().Contains("health")))
                .Filter.ByExcluding(c => c.MessageTemplate.Text.Contains("health"))
                .ReadFrom.Configuration(builder.Configuration)
                .CreateLogger());
        //}
    }
}