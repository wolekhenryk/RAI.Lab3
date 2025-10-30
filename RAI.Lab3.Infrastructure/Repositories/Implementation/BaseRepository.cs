using Microsoft.EntityFrameworkCore;
using RAI.Lab3.Domain;
using RAI.Lab3.Infrastructure.Data;
using RAI.Lab3.Infrastructure.Repositories.Interfaces;

namespace RAI.Lab3.Infrastructure.Repositories.Implementation;

public class BaseRepository<T>(AppDbContext dbContext) : IRepository<T> where T : BaseDbEntity
{
    public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.Set<T>().FindAsync([id], ct);
    }

    public async Task<List<T>> GetAllAsync(CancellationToken ct = default)
    {
        return await dbContext.Set<T>().ToListAsync(ct);
    }

    public IQueryable<T> Query()
    {
        return dbContext.Set<T>();
    }

    public async Task<T> AddAsync(T entity, CancellationToken ct = default)
    {
        if (entity.GetType() == typeof(T))
        {
            var proxy = dbContext.CreateProxy<T>();
            dbContext.Entry(proxy).CurrentValues.SetValues(entity);
            entity = proxy;
        }
        
        var entry = await dbContext.Set<T>().AddAsync(entity, ct);
        return entry.Entity;
    }

    public async Task AddAsync(IEnumerable<T> entities, CancellationToken ct = default)
    {
        await dbContext.Set<T>().AddRangeAsync(entities, ct);
    }

    public T Update(T entity)
    {
        var entry = dbContext.Set<T>().Update(entity);
        return entry.Entity;
    }

    public void Delete(T entity)
    {
        dbContext.Set<T>().Remove(entity);
    }

    public void Delete(Guid id)
    {
        var entity = dbContext.Set<T>().Find(id);
        if (entity != null)
            dbContext.Set<T>().Remove(entity);
    }
}