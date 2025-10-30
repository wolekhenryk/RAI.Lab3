using Microsoft.EntityFrameworkCore.Storage;

namespace RAI.Lab3.Infrastructure.Repositories.Interfaces;

public interface IUnitOfWork
{
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}