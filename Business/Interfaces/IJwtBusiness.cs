using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Business.Interfaces;

public interface IJwtBusiness
{
    string GenerateJwtToken(IdentityUser identityUser, IList<Claim> claims, bool isRefreshToken = false);
}