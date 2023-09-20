using Entities.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace API.Controllers.v1;

[Authorize(Roles = Roles.Admin)]
public class RoleController : BaseController
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<IdentityUser> _userManager;

    public RoleController(RoleManager<IdentityRole> roleManager,
        UserManager<IdentityUser> userManager)
    {
        _roleManager = roleManager;
        _userManager = userManager;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Create([Required] string name)
    {
        var identity = HttpContext.User.Identity as ClaimsIdentity;
        var userEmail = identity?.FindFirst(ClaimTypes.Email)?.Value;

        var user = await _userManager.FindByEmailAsync(userEmail);
        await _roleManager.CreateAsync(new IdentityRole(name));
        await _userManager.AddToRoleAsync(user, name);

        return Ok();
    }
}