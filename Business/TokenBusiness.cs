using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Business;

public class TokenBusiness
{
    public string Generate()
    {
        var handler = new JwtSecurityTokenHandler();

        var privateKey = Encoding.ASCII.GetBytes("teste");

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(privateKey),
            SecurityAlgorithms.HmacSha256Signature
        );

        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = GenerateClaims(),
            SigningCredentials = credentials,
            Expires = DateTime.UtcNow.AddHours(2),
        };

        // Generate the token
        var token = handler.CreateToken(tokenDescriptor);

        // Generate string from token
        return handler.WriteToken(token);
    }

    private static ClaimsIdentity GenerateClaims()
    {
        var claims = new ClaimsIdentity();
        claims.AddClaim(new Claim(ClaimTypes.Name, "Nome ou Id do user"));
        claims.AddClaim(new Claim(ClaimTypes.Role, "usuario normal"));
        claims.AddClaim(new Claim("Qualquer chave", "qualquer valor"));

        //User.Identity.Name
            //User.IsInRole
            return claims;
    }
}