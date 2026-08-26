using FinTrackCore.Application.Common.Models;
using FinTrackCore.Application.Features.UserInfos.Models;
using SharpOutcome;
using SharpOutcome.Helpers;

namespace FinTrackCore.Application.Features.UserInfos;

public interface IUserInfoService
{
    Task<Outcome<MutationResult, HttpBadOutcome>> CreateAsync(
        CreateUserInfoRequest request,
        CancellationToken ct);

    Task<Outcome<MutationResult, HttpBadOutcome>> UpdateAsync(
        long id,
        UpdateUserInfoRequest request,
        CancellationToken ct);

    Task<Outcome<UserInfoResponse, HttpBadOutcome>> GetByIdAsync(
        long id,
        CancellationToken ct);
}
