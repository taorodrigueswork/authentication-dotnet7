using Business.Interfaces;
using Entities.Constants;
using Entities.DTO.Request.UserIdentity;
using Microsoft.AspNetCore.Identity;
using System.Diagnostics.CodeAnalysis;

namespace API.Extensions;

[ExcludeFromCodeCoverage]
public static class IdentityInitializerSetup
{
    public static void Initialize(this IServiceScope scope)
    {
        // seed data creating roles and user admin
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

            var identityBusiness = scope.ServiceProvider.GetRequiredService<IIdentityBusiness>();

            // creating admin user
            var newUser = identityBusiness.SignUpUser(new UserSignUpDtoRequest()
                { Email = "admin@teste.com", Password = "admin123" }).Result;

            // creating roles
            if (!(roleManager.RoleExistsAsync(Roles.Admin).Result))
            {
                var result = roleManager.CreateAsync(new IdentityRole(Roles.Admin)).Result;
            }

            // add admin role to admin user
            var addUserToAdmin = userManager.AddToRoleAsync(newUser, Roles.Admin).Result;
    }
}