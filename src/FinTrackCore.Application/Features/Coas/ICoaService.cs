using FinTrackCore.Application.Common.Models;
using FinTrackCore.Application.Features.Coas.Models;
using FinTrackCore.Domain.Entities;
using SharpOutcome;
using SharpOutcome.Helpers;

namespace FinTrackCore.Application.Features.Coas;

public interface ICoaService
{
    Task<Outcome<IReadOnlyList<Coa>, HttpBadOutcome>> GetAllAsync(
        long userInfoId,
        CancellationToken ct);

    Task<Outcome<CoaListResponse, HttpBadOutcome>> GetListAsync(
        long userInfoId,
        CancellationToken ct);

    Task<Outcome<byte[], HttpBadOutcome>> ExportListPdfAsync(
        long userInfoId,
        string userDisplayName,
        CancellationToken ct);

    Task<Outcome<Coa, HttpBadOutcome>> GetByIdAsync(
        long id,
        long userInfoId,
        CancellationToken ct);

    Task<Outcome<MutationResult, HttpBadOutcome>> CreateAsync(
        long userInfoId,
        CreateCoaRequest request,
        CancellationToken ct);

    Task<Outcome<MutationResult, HttpBadOutcome>> UpdateAsync(
        long id,
        long userInfoId,
        UpdateCoaRequest request,
        CancellationToken ct);

    Task<Outcome<MutationResult, HttpBadOutcome>> DeleteAsync(
        long id,
        long userInfoId,
        CancellationToken ct);
}
