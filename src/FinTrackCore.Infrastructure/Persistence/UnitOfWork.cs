using FinTrackCore.Domain.Repositories;
using FinTrackCore.Infrastructure.Persistence;

namespace FinTrackCore.Infrastructure.Persistence;

public sealed class UnitOfWork(AppDbContext dbContext) : IUnitOfWork
{
    public async Task AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
        where TEntity : class
    {
        await dbContext.Set<TEntity>().AddAsync(entity, cancellationToken);
    }

    public void Update<TEntity>(TEntity entity)
        where TEntity : class
    {
        dbContext.Set<TEntity>().Update(entity);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
