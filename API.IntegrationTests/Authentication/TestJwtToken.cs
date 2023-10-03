using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace API.IntegrationTests.Authentication;

public class TestJwtToken
{
    public List<Claim> Claims { get; } = new();
    public int ExpiresInMinutes { get; set; } = 30;

    public TestJwtToken WithRole(string roleName)
    {
        Claims.Add(new Claim(ClaimTypes.Role, roleName));
        return this;
    }

    public TestJwtToken WithUserName(string username)
    {
        Claims.Add(new Claim(ClaimTypes.Upn, username));
        return this;
    }

    public TestJwtToken WithEmail(string email)
    {
        Claims.Add(new Claim(ClaimTypes.Email, email));
        return this;
    }

    public TestJwtToken WithExpiration(int expiresInMinutes)
    {
        ExpiresInMinutes = expiresInMinutes;
        return this;
    }

    public string Build()
    {
        var claims = new ClaimsIdentity();
        claims.AddClaims(Claims);

        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = claims,
            SigningCredentials = JwtTokenProvider.SigningCredentials,
            Expires = DateTime.Now.AddHours(6),
            Issuer = JwtTokenProvider.Issuer,
            Audience = JwtTokenProvider.Issuer
        };

        var token = JwtTokenProvider.JwtSecurityTokenHandler.CreateToken(tokenDescriptor);
        return JwtTokenProvider.JwtSecurityTokenHandler.WriteToken(token);
    }
}