using API;
using API.AppSettings;
using API.CustomMiddlewares;
using API.Extensions;
using Business;
using Business.Interfaces;
using Entities.Configurations;
using Entities.DTO.Request.Day;
using Entities.DTO.Request.Person;
using Entities.DTO.Request.Schedule;
using Entities.Entity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Interfaces;
using Persistence.Interfaces.GenericRepository;
using Persistence.Repository;
using Persistence.Repository.GenericRepository;
using Serilog;
using System.Diagnostics.CodeAnalysis;
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

        builder.Services.AddControllers().AddJsonOptions(options =>
        {   // avoid circular references when returning JSON in the API
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        });

        // Add API versioning to the application
        builder.Services.AddApiVersioning(options =>
        {
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.ReportApiVersions = true;
            options.ApiVersionReader = ApiVersionReader.Combine(new UrlSegmentApiVersionReader());
        });

        builder.Services.AddVersionedApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        // Add services to the container.
        services.AddScoped<IdentityDbContext, ApiContext>();

        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IDayRepository, DayRepository>();
        services.AddScoped<IPersonRepository, PersonRepository>();
        services.AddScoped<IDayPersonRepository, DayPersonRepository>();
        services.AddScoped<IScheduleRepository, ScheduleRepository>();

        services.AddScoped<IBusiness<PersonDtoRequest, PersonEntity>, PersonBusiness>();
        services.AddScoped<IBusiness<DayDtoRequest, DayEntity>, DayBusiness>();
        services.AddScoped<IBusiness<ScheduleDtoRequest, ScheduleEntity>, ScheduleBusiness>();
        services.AddScoped<IIdentityBusiness, IdentityBusiness>();
        services.AddScoped<IJwtBusiness, JwtBusiness>();

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

        // identity configuration
        services.AddDefaultIdentity<IdentityUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApiContext>()
            .AddDefaultTokenProviders();

        builder.Services.AddDbContext<ApiContext>(options =>
        {
            options.UseSqlServer(connectionString)
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        });

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
    }

    void ConfigureMiddleware(WebApplication app)
    {
        // Migrate latest database changes during startup
        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApiContext>();

            dbContext.Database.Migrate();
        }

        var apiVersionDescriptionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            // Show last version first in Swagger
            app.UseSwaggerUI(options =>
            {
                foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions.Reverse())
                {
                    options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
                        description.GroupName.ToUpperInvariant());
                }
            });
            app.UseDeveloperExceptionPage();
        }

        app.UseMiddleware<ExceptionHandlingMiddleware>();

        app.UseHttpsRedirection();

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