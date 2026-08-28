namespace FinTrackCore.Domain.Entities;

public class TransactionType
{
    public long Id { get; init; }

    public required string Code { get; set; }
    public required string Name { get; set; }
}
