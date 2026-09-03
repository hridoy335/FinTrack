using FinTrackCore.Domain;

namespace FinTrackCore.Application.Constants;

public static class CoaConstants
{
    public const int CodesPerAccountType = 10_000;

    public static int GetAccountTypeBaseCode(long accountTypeId) =>
        checked((int)(accountTypeId * CodesPerAccountType));

    public static string GetNextAccountCode(long accountTypeId, IEnumerable<string> existingCodes)
    {
        var baseCode = GetAccountTypeBaseCode(accountTypeId);
        var usedCodes = new HashSet<int>();

        foreach (var code in existingCodes)
        {
            if (int.TryParse(code, out var numeric)
                && numeric >= baseCode
                && numeric < baseCode + CodesPerAccountType)
            {
                usedCodes.Add(numeric);
            }
        }

        for (var candidate = baseCode; candidate < baseCode + CodesPerAccountType; candidate++)
        {
            if (!usedCodes.Contains(candidate))
            {
                return candidate.ToString();
            }
        }

        throw new InvalidOperationException(
            $"No account codes remain for account type {accountTypeId}.");
    }

    public static bool IsValidAccountTypeId(long accountTypeId) =>
        accountTypeId is AccountTypeIds.Asset
            or AccountTypeIds.Liability
            or AccountTypeIds.Equity
            or AccountTypeIds.Income
            or AccountTypeIds.Expense;
}
