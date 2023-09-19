namespace Entities.Configurations;

public record JwtOptions
{
    public required string ValidAudience { get; init; }

    public required string ValidIssuer { get; init; }

    public required string TokenExpiryTimeInHour { get; init; }

    public required string PrivateSecret { get; init; }
}