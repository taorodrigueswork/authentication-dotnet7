using Entities.Configurations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace API.Extensions;

[ExcludeFromCodeCoverage]
public static class AuthenticationSetup
{
    public static void AddAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtOptions = configuration.GetRequiredSection("Authentication:Schemes:Bearer").Get<JwtOptions>();

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtOptions.PrivateSecret)),
            SecurityAlgorithms.HmacSha256Signature
        );

        services.Configure<JwtOptions>(options =>
        {
            options.ValidIssuer = jwtOptions.ValidIssuer;
            options.ValidAudience = jwtOptions.ValidAudience;
            options.PrivateSecret = jwtOptions.PrivateSecret;
            options.SigningCredentials = credentials;
            options.TokenExpirationTimeInHour = jwtOptions.TokenExpirationTimeInHour;
            options.RefreshTokenExpirationTimeInHour = jwtOptions.RefreshTokenExpirationTimeInHour;
        });

        services.Configure<IdentityOptions>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequiredLength = 6;
        });

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.ValidIssuer,

            ValidateAudience = true,
            ValidAudience = jwtOptions.ValidAudience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtOptions.PrivateSecret)),

            RequireExpirationTime = true,
            ValidateLifetime = true,

            ClockSkew = TimeSpan.Zero
        };

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = tokenValidationParameters;
        });
    }
}