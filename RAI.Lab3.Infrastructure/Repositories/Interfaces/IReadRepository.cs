using RAI.Lab3.Domain;

namespace RAI.Lab3.Infrastructure.Repositories.Interfaces;

public interface IReadRepository<T> where T : BaseDbEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<T>> GetAllAsync(CancellationToken ct = default);
    IQueryable<T> Query();
}