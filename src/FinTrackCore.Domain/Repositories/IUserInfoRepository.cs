using FinTrackCore.Domain.Entities;

namespace FinTrackCore.Domain.Repositories;

public interface IUserInfoRepository
{
    Task<UserInfo> GetByIdAsync(long id, CancellationToken ct);
    Task<UserInfo?> GetByEmailAsync(string email, CancellationToken ct);
    Task<UserInfo?> GetByGoogleSubjectAsync(string googleSubject, CancellationToken ct);
}
