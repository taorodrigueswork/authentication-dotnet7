using System.Diagnostics.CodeAnalysis;

namespace Entities.DTO.Response.UserIdentity;

[ExcludeFromCodeCoverage]
public record UserLoginDtoResponse
{
    public required string AccessToken { get; set; }

    public required string RefreshToken { get; set; }
};