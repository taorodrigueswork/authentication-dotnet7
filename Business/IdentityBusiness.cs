using Entities.DTO.Request.UserIdentity;
using Entities.DTO.Response.UserIdentity;
using Microsoft.AspNetCore.Identity;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Business;

public class IdentityBusiness : IIdentityBusiness
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IJwtBusiness _jwtBusiness;
    //private readonly JwtOptions _jwtOptions;

    public IdentityBusiness(SignInManager<IdentityUser> signInManager,
        UserManager<IdentityUser> userManager,
        IJwtBusiness jwtBusiness)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _jwtBusiness = jwtBusiness;
        //_jwtOptions = jwtOptions.Value;
    }

    public async Task SignUpUser(UserSignUpDtoRequest userSignUpDtoRequest)
    {
        var identityUser = new IdentityUser
        {
            UserName = userSignUpDtoRequest.Email,
            Email = userSignUpDtoRequest.Email,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(identityUser, userSignUpDtoRequest.Password);
            
        if (result.Succeeded)
            await _userManager.SetLockoutEnabledAsync(identityUser, false);
    }

    public async Task<UserLoginDtoResponse> Login(UserLoginDtoRequest userLoginDtoRequest)
    {
        var result = await _signInManager.PasswordSignInAsync(userLoginDtoRequest.Email,
            userLoginDtoRequest.Password,
            false, 
            true);

        if (!result.Succeeded)
        {
            if (result.IsLockedOut)
                throw new ApplicationException("This account is blocked/locked out");
            else if (result.IsNotAllowed)
                throw new ApplicationException("This account doesn't have permission to login in this system.");
            else if (result.RequiresTwoFactor)
                throw new ApplicationException("You need to confirm login in your 2FA");
            else
                throw new ApplicationException("Username or password is wrong");
        }

        var user = await _userManager.FindByEmailAsync(userLoginDtoRequest.Email);
        var claims = await GetClaims(user);
        var jwtToken = _jwtBusiness.GenerateJwtToken(user, claims);

        return new UserLoginDtoResponse() { AccessToken = jwtToken, RefreshToken = string.Empty };
    }

    //public async Task<UsuarioLoginResponse> LoginSemSenha(string usuarioId)
    //{
    //    var usuarioLoginResponse = new UsuarioLoginResponse();
    //    var usuario = await _userManager.FindByIdAsync(usuarioId);

    //    if (await _userManager.IsLockedOutAsync(usuario))
    //        usuarioLoginResponse.AdicionarErro("Essa conta está bloqueada");
    //    else if (!await _userManager.IsEmailConfirmedAsync(usuario))
    //        usuarioLoginResponse.AdicionarErro("Essa conta precisa confirmar seu e-mail antes de realizar o login");

    //    if (usuarioLoginResponse.Sucesso)
    //        return await GerarCredenciais(usuario.Email);

    //    return usuarioLoginResponse;
    //}

    //private async Task<UsuarioLoginResponse> GerarCredenciais(string email)
    //{
    //    var user = await _userManager.FindByEmailAsync(email);
    //    var accessTokenClaims = await ObterClaims(user, adicionarClaimsUsuario: true);
    //    var refreshTokenClaims = await ObterClaims(user, adicionarClaimsUsuario: false);

    //    var dataExpiracaoAccessToken = DateTime.Now.AddSeconds(_jwtOptions.AccessTokenExpiration);
    //    var dataExpiracaoRefreshToken = DateTime.Now.AddSeconds(_jwtOptions.RefreshTokenExpiration);

    //    var accessToken = GerarToken(accessTokenClaims, dataExpiracaoAccessToken);
    //    var refreshToken = GerarToken(refreshTokenClaims, dataExpiracaoRefreshToken);

    //    return new UsuarioLoginResponse
    //    (
    //        sucesso: true,
    //        accessToken: accessToken,
    //        refreshToken: refreshToken
    //    );
    //}


    private async Task<IList<Claim>> GetClaims(IdentityUser user)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Name, user.UserName),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Nbf, DateTime.Now.ToString(CultureInfo.InvariantCulture)),
            new Claim(JwtRegisteredClaimNames.Iat, DateTime.Now.ToString(CultureInfo.InvariantCulture))
        };

        var userClaims = await _userManager.GetClaimsAsync(user);
        claims.AddRange(userClaims);

        var roles = await _userManager.GetRolesAsync(user);
        claims.AddRange(roles.Select(role => new Claim("role", role)));

        return claims;
    }
}