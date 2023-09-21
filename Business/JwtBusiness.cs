using Entities.Configurations;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Business;

public class JwtBusiness : IJwtBusiness
{
    private readonly JwtOptions _options;

    public JwtBusiness(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public string GenerateJwtToken(IdentityUser identityUser, IList<Claim> claims, bool isRefreshToken = false)
    {
        var handler = new JwtSecurityTokenHandler();

        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = GenerateClaims(identityUser, claims),
            SigningCredentials = _options.SigningCredentials,
            Expires = isRefreshToken == false ?
                DateTime.UtcNow.AddHours(Convert.ToInt16(_options.TokenExpirationTimeInHour)) : 
                DateTime.UtcNow.AddHours(Convert.ToInt16(_options.RefreshTokenExpirationTimeInHour)),
            Issuer = _options.ValidIssuer,
            Audience = _options.ValidAudience,
            NotBefore = DateTime.Now
        };

        // Generate the token
        var token = handler.CreateToken(tokenDescriptor);

        // Generate string from token
        return handler.WriteToken(token);
    }

    public string GetUserEmailFromJwtToken(string token)
    {
        var jwtToken = new JwtSecurityToken(jwtEncodedString: token);
        string userEmail = jwtToken.Claims.First(c => c.Type == "email").Value;
        return userEmail;
    }

    private static ClaimsIdentity GenerateClaims(IdentityUser identityUser, IList<Claim> claimsList)
    {
        var claims = new ClaimsIdentity();

        claims.AddClaims(claimsList);
        claims.AddClaim(new Claim(ClaimTypes.Name, identityUser.UserName));
        claims.AddClaim(new Claim(ClaimTypes.Email, identityUser.Email));
        //claims.AddClaim(new Claim("Qualquer chave", "qualquer valor"));
        
        // No controller posso usar isso para identificar o User logado
        //User.Identity.Name
        //User.IsInRole
        return claims;
    }
}