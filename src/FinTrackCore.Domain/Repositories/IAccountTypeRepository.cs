using FinTrackCore.Domain.Entities;

namespace FinTrackCore.Domain.Repositories;

public interface IAccountTypeRepository
{
    Task<AccountType> GetByIdAsync(long id, CancellationToken ct);
    Task<IReadOnlyList<AccountType>> GetAllAsync(CancellationToken ct);
}
