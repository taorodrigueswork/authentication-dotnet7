using API;
using API.CustomMiddlewares;
using API.DependencyInjection;
using API.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Serilog;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using API.Configurations;

try
{
    var builder = WebApplication.CreateBuilder(args);

    LogSetup.UseSerilog(builder);

    ConfigureServices(builder.Services);

    var app = builder.Build();

    ConfigureMiddleware(app);

    app.Logger.LogInformation("Starting app");

    app.Run();

    // Register your services/dependencies 
    void ConfigureServices(IServiceCollection services)
    {
        builder.Host.ConfigureAppSettings();

        builder.Services.AddControllers().AddJsonOptions(options =>
        {   // avoid circular references when returning JSON in the API
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        });

        // Add API versioning to the application
        builder.Services.AddVersioning();

        // Invoking action filters to validate the model state for all entities received in POST and PUT requests
        // https://code-maze.com/aspnetcore-modelstate-validation-web-api/
        services.AddScoped<ValidationFilterAttribute>();
        services.Configure<ApiBehaviorOptions>(options => options.SuppressModelStateInvalidFilter = true);

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.ConfigureOptions<SwaggerSetup>();

        builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        ArgumentNullException.ThrowIfNullOrEmpty(connectionString, "Connection string is null");

        builder.Services.AddDbContext<ApiContext>(options =>
        {
            options.UseSqlServer(connectionString)
                   .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        });

        // Add health checks
        builder.Services.AddHealthChecks(connectionString);

        // Adding JWT Bearer Authentication
        builder.Services.AddAuthentication(builder.Configuration);

        builder.Services.AddRouting(options => options.LowercaseUrls = true);

        builder.Services.AddHttpLogging();

        // Add services to the container.
        builder.Services.RegisterServices(builder.Configuration, builder.Environment);
    }

    void ConfigureMiddleware(WebApplication app)
    {
        if (!UnitTestDetector.IsInUnitTest)
        {
            // Migrate latest database changes during 
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApiContext>();
            dbContext.Database.Migrate();
            var services = scope.ServiceProvider;
            try
            {
                IdentityInitializerSetup.Initialize(services);
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred seeding the DB.");
            }
            //scope.Initialize();// TODO seed data creating roles and user admin. Refactor this using a better aproach. The await / async is not being used
        }

        // swagger config
        var apiVersionDescriptionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions.Reverse())// Show last version first in Swagger
            {
                options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
                    description.GroupName.ToUpperInvariant());
            }
        });

        // Health check general endpoint
        //app.UseHealthChecks();

        app.UseDeveloperExceptionPage();

        app.UseMiddleware<ExceptionHandlingMiddleware>();

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.UseHttpLogging();
    }
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Created to support this class to be used for integration tests purpose
[ExcludeFromCodeCoverage]
public partial class Program { }