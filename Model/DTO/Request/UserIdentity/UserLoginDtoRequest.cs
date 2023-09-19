using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Entities.DTO.Request.UserIdentity;

[ExcludeFromCodeCoverage]
public record UserLoginDtoRequest
{
    [EmailAddress]
    public required string Email { get; set; }

    public required string Password { get; set; }
};