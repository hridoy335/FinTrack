using FinTrackCore.Application.Common.Models;
using FinTrackCore.Application.Features.FinancialYears.Models;
using FinTrackCore.Domain.Entities;
using SharpOutcome;
using SharpOutcome.Helpers;

namespace FinTrackCore.Application.Features.FinancialYears;

public interface IFinancialYearService
{
    Task<Outcome<IReadOnlyList<FinancialYearListItem>, HttpBadOutcome>> GetAllAsync(
        long userInfoId,
        CancellationToken ct);

    Task<Outcome<FinancialYear, HttpBadOutcome>> GetByIdAsync(
        long id,
        long userInfoId,
        CancellationToken ct);

    Task<Outcome<FinancialYear, HttpBadOutcome>> GetCurrentAsync(
        long userInfoId,
        CancellationToken ct);

    Task<Outcome<MutationResult, HttpBadOutcome>> CreateNextAsync(
        long userInfoId,
        CancellationToken ct);

    Task<Outcome<MutationResult, HttpBadOutcome>> UpdateAsync(
        long id,
        long userInfoId,
        UpdateFinancialYearRequest request,
        CancellationToken ct);
}
