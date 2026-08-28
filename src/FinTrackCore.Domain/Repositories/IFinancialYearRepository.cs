using FinTrackCore.Domain.Entities;

namespace FinTrackCore.Domain.Repositories;

public interface IFinancialYearRepository
{
    Task<FinancialYear> GetByIdForUserAsync(long id, long userInfoId, CancellationToken ct);

    Task<FinancialYear?> GetByYearForUserAsync(int year, long userInfoId, CancellationToken ct);

    Task<IReadOnlyList<FinancialYear>> GetAllForUserAsync(long userInfoId, CancellationToken ct);

    Task<IReadOnlyList<FinancialYear>> GetTrackedAllForUserAsync(long userInfoId, CancellationToken ct);

    Task<bool> ExistsForUserAndYearAsync(int year, long userInfoId, CancellationToken ct);
}
