namespace FinTrackCore.Domain.Entities;

public class Coa
{
    public long Id { get; private set; }

    public required long UserInfoId { get; set; }
    public UserInfo? UserInfo { get; set; }

    public long? ParentId { get; set; }
    public Coa? Parent { get; set; }
    public ICollection<Coa> Children { get; set; } = new List<Coa>();

    public required long AccountTypeId { get; set; }
    public AccountType? AccountType { get; set; }

    public required string AccountCode { get; set; }
    public required string AccountName { get; set; }

    public bool IsSystemDefault { get; set; }
    public bool IsActive { get; set; }

    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
}
