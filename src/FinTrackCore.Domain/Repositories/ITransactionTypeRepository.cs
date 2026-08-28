using FinTrackCore.Domain.Entities;

namespace FinTrackCore.Domain.Repositories;

public interface ITransactionTypeRepository
{
    Task<TransactionType> GetByIdAsync(long id, CancellationToken ct);

    Task<TransactionType> GetByCodeAsync(string code, CancellationToken ct);

    Task<IReadOnlyList<TransactionType>> GetAllAsync(CancellationToken ct);
}
