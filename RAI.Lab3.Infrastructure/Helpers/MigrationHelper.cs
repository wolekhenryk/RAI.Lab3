using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RAI.Lab3.Infrastructure.Data;

namespace RAI.Lab3.Infrastructure.Helpers;

public static class MigrationHelper
{
    public static async Task MigrateAsync<TContext>(this WebApplication app) where TContext : DbContext
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();
        await dbContext.Database.MigrateAsync();
    }
    
    public static async Task ApplyConstraintsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.EnsureConstraintsCreatedAsync();
    }
}