namespace FinTrackCore.Domain.Entities;

public class FinancialYear
{
    public long Id { get; private set; }

    public required long UserInfoId { get; set; }
    public UserInfo? UserInfo { get; set; }

    public required int Year { get; set; }

    public required string Name { get; set; }

    public required DateTime StartDate { get; set; }
    public required DateTime EndDate { get; set; }

    public required bool IsActive { get; set; }
    public required bool IsClosed { get; set; }

    public required DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
}
