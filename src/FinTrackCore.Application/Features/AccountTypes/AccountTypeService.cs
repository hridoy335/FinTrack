using FinTrackCore.Domain.Entities;
using FinTrackCore.Domain.Repositories;
using SharpOutcome;
using SharpOutcome.Helpers;

namespace FinTrackCore.Application.Features.AccountTypes;

public sealed class AccountTypeService : IAccountTypeService
{
    private readonly IAccountTypeRepository _accountTypeRepository;

    public AccountTypeService(IAccountTypeRepository accountTypeRepository)
    {
        _accountTypeRepository = accountTypeRepository;
    }

    public async Task<Outcome<IReadOnlyList<AccountType>, HttpBadOutcome>> GetAllAsync(
        CancellationToken ct)
    {
        return (await _accountTypeRepository.GetAllAsync(ct)).ToList();
    }
}
