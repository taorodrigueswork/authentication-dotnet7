using API;
using API.Configurations;
using API.CustomMiddlewares;
using API.DependencyInjection;
using API.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Serilog;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json.Serialization;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    ConfigureServices(builder.Services);

    Log.Information("Starting web host");

    var app = builder.Build();

    ConfigureMiddleware(app);

    app.Run();


    // Register your services/dependencies 
    void ConfigureServices(IServiceCollection services)
    {
        builder.Host.ConfigureAppSettings();

        builder.WebHost.ConfigureKestrel(options =>
        {
            options.Listen(IPAddress.Any, 8080);
        });

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

        var seqUrl = builder.Configuration.GetRequiredSection("Seq").Get<Seq>()?.Url;
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

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

        // Add Serilog to the application https://www.youtube.com/watch?v=0acSdHJfk64
        builder.Host.UseSerilog(new LoggerConfiguration()
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .WriteTo.Console()
            .WriteTo.Debug()
            .WriteTo.Seq(serverUrl: seqUrl!)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", Serilog.Events.LogEventLevel.Warning)
            .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
            .ReadFrom.Configuration(builder.Configuration)
            .CreateLogger());

        // Add services to the container.
        builder.Services.RegisterServices(builder.Configuration);
    }

    void ConfigureMiddleware(WebApplication app)
    {
        // Migrate latest database changes during startup
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApiContext>();
        dbContext.Database.Migrate();
        //scope.Initialize();// TODO seed data creating roles and user admin. Refactor this using a better aproach. The await / async is not being used

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
        app.UseDeveloperExceptionPage();

        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseHttpsRedirection();

        // Health check general endpoint
        app.UseHealthChecks();

        app.UseAuthorization();
        app.UseSerilogRequestLogging();
        app.MapControllers();
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