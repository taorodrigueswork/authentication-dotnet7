using Microsoft.IdentityModel.Tokens;

namespace Entities.Configurations;

public class JwtOptions
{
    public required string ValidAudience { get; set; }

    public required string ValidIssuer { get; set; }

    public required string TokenExpirationTimeInHour { get; set; }

    public required string RefreshTokenExpirationTimeInHour { get; set; }

    public required string PrivateSecret { get; set; }

    public required SigningCredentials SigningCredentials { get; set; }
}