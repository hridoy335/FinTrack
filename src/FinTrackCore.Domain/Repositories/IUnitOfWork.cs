namespace FinTrackCore.Domain.Repositories;

public interface IUnitOfWork
{
    Task AddAsync<TEntity>(TEntity entity, CancellationToken ct)
        where TEntity : class;

    void Update<TEntity>(TEntity entity)
        where TEntity : class;

    void Remove<TEntity>(TEntity entity)
        where TEntity : class;

    Task<int> SaveChangesAsync(CancellationToken ct);

    Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action, CancellationToken ct);
}
