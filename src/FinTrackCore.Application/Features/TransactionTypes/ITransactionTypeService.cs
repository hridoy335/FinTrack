using FinTrackCore.Domain.Entities;
using SharpOutcome;
using SharpOutcome.Helpers;

namespace FinTrackCore.Application.Features.TransactionTypes;

public interface ITransactionTypeService
{
    Task<Outcome<IReadOnlyList<TransactionType>, HttpBadOutcome>> GetAllAsync(CancellationToken ct);

    Task<Outcome<TransactionType, HttpBadOutcome>> GetByIdAsync(long id, CancellationToken ct);
}
