using FinTrackCore.Domain.Entities;
using FinTrackCore.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FinTrackCore.Infrastructure.Persistence.Repositories;

public sealed class FinancialYearRepository : IFinancialYearRepository
{
    private readonly AppDbContext _dbContext;

    public FinancialYearRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<FinancialYear> GetByIdForUserAsync(
        long id,
        long userInfoId,
        CancellationToken ct)
    {
        var financialYear = await _dbContext.FinancialYears
            .FirstOrDefaultAsync(
                x => x.Id == id && x.UserInfoId == userInfoId,
                ct);

        return financialYear ?? throw new KeyNotFoundException();
    }

    public Task<FinancialYear?> GetByYearForUserAsync(
        int year,
        long userInfoId,
        CancellationToken ct)
    {
        return _dbContext.FinancialYears
            .FirstOrDefaultAsync(
                x => x.UserInfoId == userInfoId && x.Year == year,
                ct);
    }

    public async Task<IReadOnlyList<FinancialYear>> GetAllForUserAsync(
        long userInfoId,
        CancellationToken ct)
    {
        return await _dbContext.FinancialYears
            .AsNoTracking()
            .Where(x => x.UserInfoId == userInfoId)
            .OrderByDescending(x => x.Year)
            .ToListAsync(ct);

    }

    public async Task<IReadOnlyList<FinancialYear>> GetTrackedAllForUserAsync(
        long userInfoId,
        CancellationToken ct)
    {
        return await _dbContext.FinancialYears
            .Where(x => x.UserInfoId == userInfoId)
            .OrderByDescending(x => x.Year)
            .ToListAsync(ct);
    }

    public Task<bool> ExistsForUserAndYearAsync(
        int year,
        long userInfoId,
        CancellationToken ct)
    {
        return _dbContext.FinancialYears.AnyAsync(
            x => x.UserInfoId == userInfoId && x.Year == year,
            ct);
    }
}
