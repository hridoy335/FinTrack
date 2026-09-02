using FinTrackCore.Domain.Entities;
using SharpOutcome;
using SharpOutcome.Helpers;

namespace FinTrackCore.Application.Features.FinancialYears;

public interface IFinancialYearService
{
    Task<Outcome<IReadOnlyList<FinancialYear>, HttpBadOutcome>> GetAllAsync(
        long userInfoId,
        CancellationToken ct);

    Task<Outcome<FinancialYear, HttpBadOutcome>> GetByIdAsync(
        long id,
        long userInfoId,
        CancellationToken ct);

    Task<Outcome<FinancialYear, HttpBadOutcome>> GetCurrentAsync(
        long userInfoId,
        CancellationToken ct);
}
