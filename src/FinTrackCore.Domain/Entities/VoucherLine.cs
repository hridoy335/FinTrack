namespace FinTrackCore.Domain.Entities;

public class VoucherLine
{
    public long Id { get; private set; }

    public long TransactionId { get; set; }
    public Transaction? Transaction { get; set; }

    public required long CoaId { get; set; }
    public Coa? Coa { get; set; }

    public required int LineNumber { get; set; }
    public required decimal DebitAmount { get; set; }
    public required decimal CreditAmount { get; set; }

    public required DateTime CreatedDate { get; set; }
}
