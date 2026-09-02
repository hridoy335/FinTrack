using FinTrackCore.Domain.Entities;
using FinTrackCore.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FinTrackCore.Infrastructure.Persistence.Repositories;

public sealed class TransactionRepository : ITransactionRepository
{
    private readonly AppDbContext _dbContext;

    public TransactionRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Transaction> GetByIdForUserAsync(
        long id,
        long userInfoId,
        CancellationToken ct)
    {
        var transaction = await _dbContext.Transactions
            .AsNoTracking()
            .Include(x => x.TransactionType)
            .Include(x => x.FinancialYear)
            .Include(x => x.VoucherLines)
                .ThenInclude(x => x.Coa)
                    .ThenInclude(x => x!.AccountType)
            .FirstOrDefaultAsync(
                x => x.Id == id && x.UserInfoId == userInfoId,
                ct);

        return transaction ?? throw new KeyNotFoundException();
    }

    public async Task<(IReadOnlyList<Transaction> Items, long TotalCount)> GetPagedForUserAsync(
        long userInfoId,
        long? financialYearId,
        long? transactionTypeId,
        DateTime? fromDate,
        DateTime? toDate,
        int page,
        int pageSize,
        CancellationToken ct)
    {
        var query = _dbContext.Transactions
            .AsNoTracking()
            .Include(x => x.TransactionType)
            .Include(x => x.FinancialYear)
            .Where(x => x.UserInfoId == userInfoId);

        if (financialYearId is not null)
        {
            query = query.Where(x => x.FinancialYearId == financialYearId.Value);
        }

        if (transactionTypeId is not null)
        {
            query = query.Where(x => x.TransactionTypeId == transactionTypeId.Value);
        }

        if (fromDate is not null)
        {
            query = query.Where(x => x.TransactionDate >= fromDate.Value);
        }

        if (toDate is not null)
        {
            query = query.Where(x => x.TransactionDate <= toDate.Value);
        }

        var totalCount = await query.LongCountAsync(ct);

        var items = await query
            .OrderByDescending(x => x.TransactionDate)
            .ThenByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }
}
