namespace FinTrackCore.Application.Features.Coas.Models;

public sealed class CreateCoaRequest
{
    public long? ParentId { get; init; }
    public required long AccountTypeId { get; init; }
    public required string AccountName { get; init; }
}

public sealed class UpdateCoaRequest
{
    public long? ParentId { get; init; }
    public required string AccountName { get; init; }
    public bool IsActive { get; init; } = true;
}
