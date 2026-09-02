namespace FinTrackCore.Domain.Entities;

public class AccountType
{
    public long Id { get; init; }

    public required string Code { get; set; }
    public required string Name { get; set; }
    public required string NormalBalance { get; set; }
}
