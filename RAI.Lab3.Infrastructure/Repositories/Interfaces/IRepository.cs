using RAI.Lab3.Domain;

namespace RAI.Lab3.Infrastructure.Repositories.Interfaces;

public interface IRepository<T> : IReadRepository<T> where T : BaseDbEntity
{
    Task<T> AddAsync(T entity, CancellationToken ct = default);
    Task AddAsync(IEnumerable<T> entities, CancellationToken ct = default);
    
    T Update(T entity);
    
    void Delete(T entity);
    void Delete(Guid id);
}