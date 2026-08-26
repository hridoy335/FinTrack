using FinTrackCore.Domain.Entities;
using FinTrackCore.Domain.Repositories;
using FinTrackCore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinTrackCore.Infrastructure.Persistence.Repositories;

public sealed class AccountTypeRepository : IAccountTypeRepository
{
    private readonly AppDbContext _dbContext;

    public AccountTypeRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AccountType> GetByIdAsync(long id, CancellationToken ct)
    {
        var accountType = await _dbContext.AccountTypes
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        return accountType ?? throw new KeyNotFoundException();
    }

    public async Task<IReadOnlyList<AccountType>> GetAllAsync(CancellationToken ct)
    {
        return await _dbContext.AccountTypes
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .ToListAsync(ct);
    }
}
