namespace FinTrackCore.Application.Features.Coas.Models;

public sealed class CoaListResponse
{
    public required IReadOnlyList<CoaListSectionResponse> Sections { get; init; }
}

public sealed class CoaListSectionResponse
{
    public required long AccountTypeId { get; init; }
    public required string AccountTypeCode { get; init; }
    public required string AccountTypeName { get; init; }
    public required IReadOnlyList<CoaListItemResponse> Items { get; init; }
}

public sealed class CoaListItemResponse
{
    public required long Id { get; init; }
    public required string Code { get; init; }
    public required string AccountHeadName { get; init; }
    public long? ParentId { get; init; }
    public required bool IsSystemDefault { get; init; }
    public required bool IsActive { get; init; }
    public required bool CanEdit { get; init; }
    public required bool CanDelete { get; init; }
}
