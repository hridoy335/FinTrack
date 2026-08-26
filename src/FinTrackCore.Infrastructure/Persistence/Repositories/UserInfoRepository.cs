using FinTrackCore.Domain.Entities;
using FinTrackCore.Domain.Repositories;
using FinTrackCore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinTrackCore.Infrastructure.Persistence.Repositories;

public sealed class UserInfoRepository : IUserInfoRepository
{
    private readonly AppDbContext _dbContext;

    public UserInfoRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UserInfo> GetByIdAsync(long id, CancellationToken ct)
    {
        var userInfo = await _dbContext.UserInfos
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        return userInfo ?? throw new KeyNotFoundException();
    }

    public Task<UserInfo?> GetByUserNameAsync(string userName, CancellationToken ct)
    {
        return _dbContext.UserInfos.FirstOrDefaultAsync(x => x.UserName == userName, ct);
    }

    public Task<UserInfo?> GetByEmailAsync(string email, CancellationToken ct)
    {
        return _dbContext.UserInfos.FirstOrDefaultAsync(x => x.Email == email, ct);
    }

    public Task<UserInfo?> GetByGoogleSubjectAsync(string googleSubject, CancellationToken ct)
    {
        return _dbContext.UserInfos.FirstOrDefaultAsync(x => x.GoogleSubject == googleSubject, ct);
    }
}
