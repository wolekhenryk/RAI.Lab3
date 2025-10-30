using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using RAI.Lab3.Domain.Models;
using RAI.Lab3.Infrastructure.Roles;

namespace RAI.Lab3.Infrastructure.Seeding;

public static class IdentitySeeder
{
    public static async Task SeedAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        foreach (var role in AppRoles.AllRoles)
        {
            if (await roleManager.RoleExistsAsync(role)) 
                continue;
            
            var identityRole = new IdentityRole<Guid>(role);
            await roleManager.CreateAsync(identityRole);
        }
        
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

        const string adminEmail = "admin@mail.com";
        const string adminPassword = "Admin123!";
        
        var adminUser = await userManager.FindByNameAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new User
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };
            await userManager.CreateAsync(adminUser, adminPassword);
            await userManager.AddToRoleAsync(adminUser, AppRoles.Teacher);
        }
    }
}