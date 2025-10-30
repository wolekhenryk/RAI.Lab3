using Microsoft.EntityFrameworkCore.Storage;
using RAI.Lab3.Infrastructure.Data;
using RAI.Lab3.Infrastructure.Repositories.Interfaces;

namespace RAI.Lab3.Infrastructure.Repositories.Implementation;

public class UnitOfWork(AppDbContext dbContext) : IUnitOfWork
{
    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default)
    {
        return await dbContext.Database.BeginTransactionAsync(ct);
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await dbContext.SaveChangesAsync(ct);
    }
}