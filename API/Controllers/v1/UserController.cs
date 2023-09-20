using Business;
using Business.Interfaces;
using Entities.DTO.Request.UserIdentity;
using Entities.DTO.Response.UserIdentity;
using Entities.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;

namespace API.Controllers.v1;

public class UserController : BaseController
{
    private readonly IIdentityBusiness _identityBusiness;

    public UserController(IIdentityBusiness identityBusiness) =>
        _identityBusiness = identityBusiness;

    /// <summary>
    /// Create a new user.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <param name="userSignUpDtoRequest">User data</param>
    /// <returns></returns>
    /// <response code="200">User was created successfully</response>
    [HttpPost("sign-up")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(IdentityUser))]
    [ServiceFilter(typeof(ValidationFilterAttribute))]
    public async Task<IActionResult> SignUp(UserSignUpDtoRequest userSignUpDtoRequest)
    {
        return Created(string.Empty, await _identityBusiness.SignUpUser(userSignUpDtoRequest));
    }

    /// <summary>
    /// Creates tokens for the username and password provided.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <param name="userLoginDtoRequest">User data to authenticate</param>
    /// <returns></returns>
    /// <response code="200">User was valid and tokens were created</response>
    /// <response code="401">Something is wrong with password and user provided</response>
    [ProducesResponseType(typeof(UserLoginDtoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [HttpPost("authenticate")]
    public async Task<ActionResult<UserLoginDtoResponse>> Authenticate(UserLoginDtoRequest userLoginDtoRequest)
    {
        return Ok(await _identityBusiness.Authenticate(userLoginDtoRequest));
    }

    /// <summary>
    /// uses the refresh token to generate new valid tokens and refresh token.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <returns></returns>
    /// <response code="200">A new token was generated as well a refresh token</response>
    /// <response code="401">Refresh token doesn't have authorization</response>
    [ProducesResponseType(typeof(UserLoginDtoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Authorize]
    [HttpPost("refresh-token")]
    public async Task<ActionResult<UserLoginDtoResponse>> RefreshLogin()
    {
        var identity = HttpContext.User.Identity as ClaimsIdentity;

        var userEmail = identity?.FindFirst(ClaimTypes.Email)?.Value;

        return Ok(await _identityBusiness.RefreshToken(userEmail));
    }
}