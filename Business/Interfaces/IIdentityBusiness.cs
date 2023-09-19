using Entities.DTO.Request.UserIdentity;
using Entities.DTO.Response.UserIdentity;

namespace Business.Interfaces;

public interface IIdentityBusiness
{
    /// <summary>
    /// Create a new user in the database, with the flag EmailConfirmed set to true and SetLockoutEnabledAsync set to true as well.
    /// </summary>
    /// <param name="userSignUpDtoRequest"></param>
    /// <returns></returns>
    Task SignUpUser(UserSignUpDtoRequest userSignUpDtoRequest);

    /// <summary>
    /// Makes the user login and generate JWT Access Token and Refresh Token.
    /// </summary>
    /// <param name="userLoginDtoRequest"></param>
    /// <returns></returns>
    Task<UserLoginDtoResponse> Login(UserLoginDtoRequest userLoginDtoRequest);
}