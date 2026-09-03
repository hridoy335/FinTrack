using FinTrackCore.Application.Constants;
using FinTrackCore.Domain;

namespace FinTrackCore.Application.Tests.Constants;

public sealed class CoaConstantsTests
{
    [Fact]
    public void GetNextAccountCode_uses_first_available_slot_in_account_type_range()
    {
        var nextCode = CoaConstants.GetNextAccountCode(
            AccountTypeIds.Equity,
            ["30000", "30100", "30200", "30300"]);

        Assert.Equal("30001", nextCode);
    }

    [Fact]
    public void GetNextAccountCode_continues_sequential_codes_for_assets()
    {
        var nextCode = CoaConstants.GetNextAccountCode(
            AccountTypeIds.Asset,
            ["10000", "10001", "10002"]);

        Assert.Equal("10003", nextCode);
    }
}
