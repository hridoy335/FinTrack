using FinTrackCore.Domain.Entities;
using FinTrackCore.Domain.Repositories;
using FinTrackCore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinTrackCore.Infrastructure.Persistence.Repositories;

public sealed class CoaRepository : ICoaRepository
{
    private readonly AppDbContext _dbContext;

    public CoaRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Coa> GetByIdForUserAsync(
        long id,
        long userInfoId,
        CancellationToken ct)
    {
        var coa = await _dbContext.Coas
            .Include(x => x.AccountType)
            .FirstOrDefaultAsync(
                x => x.Id == id && x.UserInfoId == userInfoId,
                ct);

        return coa ?? throw new KeyNotFoundException();
    }

    public Task<bool> ExistsByCodeForUserAsync(
        string accountCode,
        long userInfoId,
        CancellationToken ct)
    {
        return _dbContext.Coas.AnyAsync(
            x => x.UserInfoId == userInfoId && x.AccountCode == accountCode,
            ct);
    }

    public Task<bool> ExistsByAccountNameForUserAndAccountTypeAsync(
        long userInfoId,
        long accountTypeId,
        string accountName,
        long? excludeCoaId,
        CancellationToken ct)
    {
        var normalizedName = accountName.Trim().ToLower();

        return _dbContext.Coas.AnyAsync(
            x => x.UserInfoId == userInfoId
                 && x.AccountTypeId == accountTypeId
                 && x.AccountName.ToLower() == normalizedName
                 && (excludeCoaId == null || x.Id != excludeCoaId),
            ct);
    }

    public async Task<IReadOnlyList<string>> GetAccountCodesForUserAndAccountTypeAsync(
        long userInfoId,
        long accountTypeId,
        CancellationToken ct)
    {
        return await _dbContext.Coas
            .AsNoTracking()
            .Where(x => x.UserInfoId == userInfoId && x.AccountTypeId == accountTypeId)
            .Select(x => x.AccountCode)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Coa>> GetAllForUserAsync(
        long userInfoId,
        CancellationToken ct)
    {
        return await _dbContext.Coas
            .AsNoTracking()
            .Include(x => x.AccountType)
            .Where(x => x.UserInfoId == userInfoId)
            .OrderBy(x => x.AccountCode)
            .ToListAsync(ct);
    }

    public Task<bool> HasChildrenAsync(
        long id,
        long userInfoId,
        CancellationToken ct)
    {
        return _dbContext.Coas.AnyAsync(
            x => x.ParentId == id && x.UserInfoId == userInfoId,
            ct);
    }

    public async Task<IReadOnlySet<long>> GetCoaIdsUsedInTransactionsAsync(
        long userInfoId,
        CancellationToken ct)
    {
        var coaIds = await (
            from line in _dbContext.VoucherLines.AsNoTracking()
            join transaction in _dbContext.Transactions.AsNoTracking()
                on line.TransactionId equals transaction.Id
            where transaction.UserInfoId == userInfoId
            select line.CoaId)
            .Distinct()
            .ToListAsync(ct);

        return coaIds.ToHashSet();
    }

    public Task<bool> IsUsedInTransactionsAsync(
        long coaId,
        long userInfoId,
        CancellationToken ct)
    {
        return (
            from line in _dbContext.VoucherLines.AsNoTracking()
            join transaction in _dbContext.Transactions.AsNoTracking()
                on line.TransactionId equals transaction.Id
            where line.CoaId == coaId && transaction.UserInfoId == userInfoId
            select line.Id)
            .AnyAsync(ct);
    }
}
