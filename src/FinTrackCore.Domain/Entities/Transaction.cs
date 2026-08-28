namespace FinTrackCore.Domain.Entities;

public class Transaction
{
    public long Id { get; private set; }

    public required long UserInfoId { get; set; }
    public UserInfo? UserInfo { get; set; }

    public required long FinancialYearId { get; set; }
    public FinancialYear? FinancialYear { get; set; }

    public required long TransactionTypeId { get; set; }
    public TransactionType? TransactionType { get; set; }

    public required DateTime TransactionDate { get; set; }
    public required decimal Amount { get; set; }
    public string? Description { get; set; }

    public required DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }

    public ICollection<VoucherLine> VoucherLines { get; set; } = new List<VoucherLine>();
}
