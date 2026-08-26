namespace FinTrackCore.Domain;

public static class AccountTypeIds
{
    public const long Asset = 1;
    public const long Liability = 2;
    public const long Equity = 3;
    public const long Income = 4;
    public const long Expense = 5;
}

public static class NormalBalance
{
    public const string Debit = "DEBIT";
    public const string Credit = "CREDIT";
}
