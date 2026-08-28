using FinTrackCore.Domain.Entities;
using FinTrackCore.Domain.Repositories;
using FinTrackCore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinTrackCore.Infrastructure.Persistence.Repositories;

public sealed class TransactionTypeRepository : ITransactionTypeRepository
{
    private readonly AppDbContext _dbContext;

    public TransactionTypeRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TransactionType> GetByIdAsync(long id, CancellationToken ct)
    {
        var transactionType = await _dbContext.TransactionTypes
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        return transactionType ?? throw new KeyNotFoundException();
    }

    public async Task<TransactionType> GetByCodeAsync(string code, CancellationToken ct)
    {
        var transactionType = await _dbContext.TransactionTypes
            .FirstOrDefaultAsync(x => x.Code == code, ct);

        return transactionType ?? throw new KeyNotFoundException();
    }

    public async Task<IReadOnlyList<TransactionType>> GetAllAsync(CancellationToken ct)
    {
        return await _dbContext.TransactionTypes
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .ToListAsync(ct);
    }
}
