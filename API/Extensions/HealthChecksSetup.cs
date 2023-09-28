using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Diagnostics.CodeAnalysis;

namespace API.Extensions;

[ExcludeFromCodeCoverage]
public static class HealthChecksSetup
{
    public static void AddHealthChecks(this IServiceCollection services, string connectionString)
    {
        services.AddHealthChecks()
            .AddSqlServer(connectionString, name: "Sql Server Database");

        services.AddHealthChecksUI(setupSettings: setup =>
        {
            // avoid throwing exception about ssl 
            setup.UseApiEndpointHttpMessageHandler(sp =>
            {
                return new HttpClientHandler
                {
                    ClientCertificateOptions = ClientCertificateOption.Manual,
                    ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) => { return true; }
                };
            });

            setup.SetEvaluationTimeInSeconds(5); // Configures the UI to poll for health check updates every 5 seconds
            setup.SetApiMaxActiveRequests(1); //Only one active request will be executed at a time.
            //All the excedent requests will result in 429(Too many requests)
            setup.MaximumHistoryEntriesPerEndpoint(50); // Set the maximum history entries by endpoint that will be served by the UI api middleware
            
            setup.AddHealthCheckEndpoint("Basic Health Check", "/health");
        }).AddInMemoryStorage();
    }

    public static void UseHealthChecks(this IApplicationBuilder app)
    {
        app.UseHealthChecks("/health", new HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
            ResultStatusCodes =
            {
                [HealthStatus.Healthy] = StatusCodes.Status200OK,
                [HealthStatus.Degraded] = StatusCodes.Status500InternalServerError,
                [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable,
            },
        }).UseHealthChecksUI(setup =>
        {
            setup.ApiPath = "/healthcheck";
            setup.UIPath = "/healthcheck-ui";
        });
    }
}