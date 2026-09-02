using FinTrackCore.Application.Features.Reports.Models;
using SharpOutcome;
using SharpOutcome.Helpers;

namespace FinTrackCore.Application.Features.Reports;

public interface IReportService
{
    Task<Outcome<DashboardResponse, HttpBadOutcome>> GetDashboardAsync(
        long userInfoId,
        long? financialYearId,
        CancellationToken ct);
}
