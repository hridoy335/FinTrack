using FinTrackCore.Domain.Entities;
using FinTrackCore.Domain.Repositories;

namespace FinTrackCore.Application.Features.FinancialYears;

public interface IDefaultFinancialYearSeedService
{
    Task SeedForUserAsync(long userInfoId, CancellationToken ct);
}
