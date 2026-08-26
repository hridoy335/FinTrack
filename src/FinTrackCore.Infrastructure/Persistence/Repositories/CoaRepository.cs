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
}
