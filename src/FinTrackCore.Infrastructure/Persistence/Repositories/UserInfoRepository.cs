using FinTrackCore.Domain.Entities;
using FinTrackCore.Domain.Repositories;
using FinTrackCore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinTrackCore.Infrastructure.Persistence.Repositories;

public sealed class UserInfoRepository(AppDbContext dbContext) : IUserInfoRepository
{
    public async Task<UserInfo?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var query =
            from userInfo in dbContext.UserInfos
            where userInfo.Id == id
            select userInfo;

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<UserInfo?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default)
    {
        var query =
            from userInfo in dbContext.UserInfos
            where userInfo.UserName == userName
            select userInfo;

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<UserInfo?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var query =
            from userInfo in dbContext.UserInfos
            where userInfo.Email == email
            select userInfo;

        return await query.FirstOrDefaultAsync(cancellationToken);
    }
}
