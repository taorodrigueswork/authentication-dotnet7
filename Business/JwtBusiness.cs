using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Entities.Configurations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Business;

public class JwtBusiness : IJwtBusiness
{
   // private readonly IConfiguration _configuration;
    private readonly JwtOptions _options;

    public JwtBusiness(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public string GenerateJwtToken(IdentityUser identityUser, IList<Claim> claims)
    {
        // get configuration from appsettings.json
        //_configuration.GetRequiredSection("Seq").Get<JwtOptions>()
        //var validAudience = _configuration.GetRequiredSection("Bearer:ValidAudience");
        //var validIssuer = _configuration.GetRequiredSection("Bearer:ValidIssuer");
        //var privateKey = Encoding.ASCII.GetBytes(_configuration.GetRequiredSection("Bearer:PrivateSecret").Value!);

        var privateKey = Encoding.ASCII.GetBytes(_options.PrivateSecret);
        
        var handler = new JwtSecurityTokenHandler();

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(privateKey),
            SecurityAlgorithms.HmacSha256Signature
        );

        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = GenerateClaims(identityUser, claims),
            SigningCredentials = credentials,
            Expires = DateTime.UtcNow.AddHours(Convert.ToInt16(_options.TokenExpiryTimeInHour)),
            Issuer = _options.ValidIssuer,
            Audience = _options.ValidAudience,
            NotBefore = DateTime.Now
        };

        // Generate the token
        var token = handler.CreateToken(tokenDescriptor);

        // Generate string from token
        return handler.WriteToken(token);
    }

    private static ClaimsIdentity GenerateClaims(IdentityUser identityUser, IList<Claim> claimsList)
    {
        var claims = new ClaimsIdentity();

        claims.AddClaims(claimsList);
        claims.AddClaim(new Claim(ClaimTypes.Name, identityUser.UserName));
        //claims.AddClaim(new Claim("Qualquer chave", "qualquer valor"));
        
        // No controller posso usar isso para identificar o User logado
        //User.Identity.Name
        //User.IsInRole
        return claims;
    }
}