namespace FinTrackCore.Application.Features.Coas;

public interface IDefaultCoaSeedService
{
    Task SeedForUserAsync(long userInfoId, CancellationToken ct);
}
