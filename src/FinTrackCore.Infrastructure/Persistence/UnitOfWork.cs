using FinTrackCore.Domain.Repositories;
using FinTrackCore.Infrastructure.Persistence;

namespace FinTrackCore.Infrastructure.Persistence;

public sealed class UnitOfWork(AppDbContext dbContext) : IUnitOfWork
{
    public async Task AddAsync<TEntity>(TEntity entity, CancellationToken ct)
        where TEntity : class
    {
        await dbContext.Set<TEntity>().AddAsync(entity, ct);
    }

    public void Update<TEntity>(TEntity entity)
        where TEntity : class
    {
        dbContext.Set<TEntity>().Update(entity);
    }

    public void Remove<TEntity>(TEntity entity)
        where TEntity : class
    {
        dbContext.Set<TEntity>().Remove(entity);
    }

    public Task<int> SaveChangesAsync(CancellationToken ct)
    {
        return dbContext.SaveChangesAsync(ct);
    }
}
