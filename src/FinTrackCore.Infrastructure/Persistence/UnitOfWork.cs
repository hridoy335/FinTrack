using FinTrackCore.Domain.Repositories;
using FinTrackCore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Storage;

namespace FinTrackCore.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _dbContext;

    public UnitOfWork(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync<TEntity>(TEntity entity, CancellationToken ct)
        where TEntity : class
    {
        await _dbContext.Set<TEntity>().AddAsync(entity, ct);
    }

    public void Update<TEntity>(TEntity entity)
        where TEntity : class
    {
        _dbContext.Set<TEntity>().Update(entity);
    }

    public void Remove<TEntity>(TEntity entity)
        where TEntity : class
    {
        _dbContext.Set<TEntity>().Remove(entity);
    }

    public Task<int> SaveChangesAsync(CancellationToken ct)
    {
        return _dbContext.SaveChangesAsync(ct);
    }

    public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action, CancellationToken ct)
    {
        if (_dbContext.Database.CurrentTransaction is not null)
        {
            await action(ct);
            return;
        }

        await using IDbContextTransaction transaction = await _dbContext.Database.BeginTransactionAsync(ct);

        try
        {
            await action(ct);
            await transaction.CommitAsync(ct);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }
}
