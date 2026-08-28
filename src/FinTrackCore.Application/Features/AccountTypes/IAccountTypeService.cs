using FinTrackCore.Domain.Entities;
using SharpOutcome;
using SharpOutcome.Helpers;

namespace FinTrackCore.Application.Features.AccountTypes;

public interface IAccountTypeService
{
    Task<Outcome<IReadOnlyList<AccountType>, HttpBadOutcome>> GetAllAsync(CancellationToken ct);
}
