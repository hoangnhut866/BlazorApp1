using BlazorApp1.Api.Data;
using Microsoft.AspNetCore.Identity;

namespace BlazorApp1.Api.Services;

public static class DatabaseSeeder
{
    private const string AdminRole = "Admin";
    private const string MemberRole = "Member";

    private const string AdminEmail = "admin@example.com";
    private const string AdminPassword = "Admin@123!";

    private const string MemberEmail = "member@example.com";
    private const string MemberPassword = "Member@123!";

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        await EnsureRoleAsync(roleManager, AdminRole);
        await EnsureRoleAsync(roleManager, MemberRole);

        await EnsureUserAsync(userManager, AdminEmail, AdminPassword, AdminRole);
        await EnsureUserAsync(userManager, MemberEmail, MemberPassword, MemberRole);
    }

    private static async Task EnsureRoleAsync(RoleManager<IdentityRole> roleManager, string roleName)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            var result = await roleManager.CreateAsync(new IdentityRole(roleName));
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to create role '{roleName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
    }

    private static async Task EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string password,
        string role)
    {
        if (await userManager.FindByEmailAsync(email) is not null)
            return;

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"Failed to create user '{email}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        await userManager.AddToRoleAsync(user, role);
    }
}
