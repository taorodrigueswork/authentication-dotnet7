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

    public const string REFRESHTOKEN = nameof(REFRESHTOKEN);
    public const string API = nameof(API);

    public IdentityBusiness(SignInManager<IdentityUser> signInManager,
        UserManager<IdentityUser> userManager,
        IJwtBusiness jwtBusiness)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _jwtBusiness = jwtBusiness;
    }

    public async Task<IdentityUser> SignUpUser(UserSignUpDtoRequest userSignUpDtoRequest)
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

        return await _userManager.FindByEmailAsync(identityUser.Email) ?? throw new ApplicationException("User was not created");
    }

    public async Task<UserLoginDtoResponse> Authenticate(UserLoginDtoRequest userLoginDtoRequest)
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

        return await GenerateTokens(user, claims);
    }

    public async Task<UserLoginDtoResponse> RefreshToken(string refreshToken)
    {
        var userEmail = _jwtBusiness.GetUserEmailFromJwtToken(refreshToken);
        var user = await _userManager.FindByEmailAsync(userEmail);

        if (await _userManager.IsLockedOutAsync(user))
            throw new ApplicationException("This account is blocked/locked out");

        var claims = await GetClaims(user);
        
        // delete old refresh token
        await _userManager.RemoveAuthenticationTokenAsync(user, API, nameof(REFRESHTOKEN));

        return await GenerateTokens(user, claims);
    }

    private async Task<UserLoginDtoResponse> GenerateTokens(IdentityUser user, IList<Claim> claims)
    {
        var jwtToken = _jwtBusiness.GenerateJwtToken(user, claims);
        var refreshToken = _jwtBusiness.GenerateJwtToken(user, new List<Claim>(), true);

        await _userManager.SetAuthenticationTokenAsync(user, API, REFRESHTOKEN, refreshToken);

        return new UserLoginDtoResponse() { AccessToken = jwtToken, RefreshToken = refreshToken };
    }

    private async Task<IList<Claim>> GetClaims(IdentityUser user)
    {
        var claims = new List<Claim>
        {
            new (JwtRegisteredClaimNames.Sub, user.Id),
            new (JwtRegisteredClaimNames.Name, user.UserName),
            new (JwtRegisteredClaimNames.Email, user.Email),
            new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new (JwtRegisteredClaimNames.Nbf, DateTime.Now.ToString(CultureInfo.InvariantCulture)),
            new (JwtRegisteredClaimNames.Iat, DateTime.Now.ToString(CultureInfo.InvariantCulture))
        };

        var userClaims = await _userManager.GetClaimsAsync(user);
        claims.AddRange(userClaims);

        var roles = await _userManager.GetRolesAsync(user);
        claims.AddRange(roles.Select(role => new Claim("role", role)));

        return claims;
    }
}