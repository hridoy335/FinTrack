using FinTrackCore.Domain.Entities;
using FinTrackCore.Domain.Repositories;
using SharpOutcome;
using SharpOutcome.Helpers;

namespace FinTrackCore.Application.Features.TransactionTypes;

public sealed class TransactionTypeService : ITransactionTypeService
{
    private readonly ITransactionTypeRepository _transactionTypeRepository;

    public TransactionTypeService(ITransactionTypeRepository transactionTypeRepository)
    {
        _transactionTypeRepository = transactionTypeRepository;
    }

    public async Task<Outcome<IReadOnlyList<TransactionType>, HttpBadOutcome>> GetAllAsync(
        CancellationToken ct)
    {
        return (await _transactionTypeRepository.GetAllAsync(ct)).ToList();
    }

    public async Task<Outcome<TransactionType, HttpBadOutcome>> GetByIdAsync(
        long id,
        CancellationToken ct)
    {
        return await _transactionTypeRepository.GetByIdAsync(id, ct);
    }
}
