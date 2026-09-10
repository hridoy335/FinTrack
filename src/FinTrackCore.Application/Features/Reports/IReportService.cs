using FinTrackCore.Application.Features.Reports.Models;
using SharpOutcome;
using SharpOutcome.Helpers;

namespace FinTrackCore.Application.Features.Reports;

public interface IReportService
{
    Task<Outcome<DashboardResponse, HttpBadOutcome>> GetDashboardAsync(
        long userInfoId,
        DashboardReportQuery query,
        CancellationToken ct);

    Task<Outcome<CashflowReportResponse, HttpBadOutcome>> GetCashflowAsync(
        long userInfoId,
        CashflowReportQuery query,
        CancellationToken ct);

    Task<Outcome<BalanceReportResponse, HttpBadOutcome>> GetBalanceAsync(
        long userInfoId,
        BalanceReportQuery query,
        CancellationToken ct);

    Task<Outcome<AccountStatementResponse, HttpBadOutcome>> GetAccountStatementAsync(
        long userInfoId,
        AccountStatementQuery query,
        CancellationToken ct);

    Task<Outcome<MonthlyCashflowReportResponse, HttpBadOutcome>> GetMonthlyCashflowAsync(
        long userInfoId,
        MonthlyCashflowReportQuery query,
        CancellationToken ct);
}
