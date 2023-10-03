using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Persistence.Context;
using System.Data.Common;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;
using AutoFixture;
using AutoFixture.NUnit3;
using Entities.Entity;
using NUnit.Framework;

namespace IntegrationTests.Authentication;

public abstract class BaseConfigurationIntegrationTest : IAsyncDisposable
{
    protected WebApplicationFactory<Program> Factory;
    protected HttpClient Client;
    protected ApiContext? Context;
    protected Fixture? Fixture;

    protected BaseConfigurationIntegrationTest()
    {
        Factory = new WebApplicationFactory<Program>();
        Factory = Factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                _ = services.RemoveAll(typeof(DbContextOptions));
                _ = services.RemoveAll(typeof(ApiContext));
                _ = services.RemoveAll(typeof(DbContextOptions<ApiContext>));
                _ = services.RemoveAll(typeof(DbConnection));

                services.AddSingleton<DbConnection, SqliteConnection>(container =>
                {
                    var connection = new SqliteConnection("DataSource=:memory:");
                    connection.Open();

                    return connection;
                });

                services.AddDbContext<ApiContext>((container, options) =>
                {
                    var connection = container.GetRequiredService<DbConnection>();
                    options.UseSqlite(connection);
                    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                });

                services.Configure<JwtBearerOptions>(
                    JwtBearerDefaults.AuthenticationScheme,
                    options =>
                    {
                        options.Configuration = new OpenIdConnectConfiguration
                        {
                            Issuer = JwtTokenProvider.Issuer,
                        };
                        options.TokenValidationParameters.ValidIssuer = JwtTokenProvider.Issuer;
                        options.TokenValidationParameters.ValidAudience = JwtTokenProvider.Issuer;
                        options.Configuration.SigningKeys.Add(JwtTokenProvider.SecurityKey);
                    }
                );
            });
        });
        Client = Factory.CreateClient(new WebApplicationFactoryClientOptions()
        {
            AllowAutoRedirect = false
        });
    }

    [SetUp]
    public void SetUp()
    {
        var scope = Factory.Services.CreateScope();
        Context = scope.ServiceProvider.GetRequiredService<ApiContext>()!;
        Context?.Database.EnsureCreated();
        Fixture = CustomAutoDataAttribute.CreateOmitOnRecursionFixture();
    }

    //	Marks a method that should be called after each test method. One such method should be present before each test class.
    [TearDown]
    public void TearDown()
    {
        Context?.Database.EnsureDeleted();
    }

    [OneTimeTearDown]
    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        await Factory.DisposeAsync();
    }
}