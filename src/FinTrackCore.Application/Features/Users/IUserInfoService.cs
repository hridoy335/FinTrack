using FinTrackCore.Application.Common.Models;
using FinTrackCore.Application.Features.Users.Models;
using SharpOutcome;
using SharpOutcome.Helpers;

namespace FinTrackCore.Application.Features.Users;

public interface IUserInfoService
{
    Task<Outcome<MutationResult, HttpBadOutcome>> CreateAsync(
        CreateUserInfoRequest request,
        CancellationToken cancellationToken = default);

    Task<Outcome<MutationResult, HttpBadOutcome>> UpdateAsync(
        long id,
        UpdateUserInfoRequest request,
        CancellationToken cancellationToken = default);

    Task<Outcome<UserInfoResponse, HttpBadOutcome>> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default);
}
