using FinTrackCore.Application.Common.Models;
using FinTrackCore.Application.Features.Transactions.Models;
using FinTrackCore.Domain.Entities;
using SharpOutcome;
using SharpOutcome.Helpers;

namespace FinTrackCore.Application.Features.Transactions;

public interface ITransactionService
{
    Task<Outcome<(IReadOnlyList<Transaction> Items, long TotalCount), HttpBadOutcome>> GetAllAsync(
        long userInfoId,
        TransactionListQuery query,
        CancellationToken ct);

    Task<Outcome<Transaction, HttpBadOutcome>> GetByIdAsync(
        long id,
        long userInfoId,
        CancellationToken ct);

    Task<Outcome<MutationResult, HttpBadOutcome>> CreateAsync(
        long userInfoId,
        CreateTransactionRequest request,
        CancellationToken ct);
}
