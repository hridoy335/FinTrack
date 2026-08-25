using FinTrackCore.Domain.Entities;

namespace FinTrackCore.Domain.Repositories;

public interface IUserInfoRepository
{
    Task<UserInfo?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<UserInfo?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default);
    Task<UserInfo?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
}
