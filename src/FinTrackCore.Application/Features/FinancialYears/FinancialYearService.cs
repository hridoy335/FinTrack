using FinTrackCore.Application.Common.Configuration;
using FinTrackCore.Application.Constants;
using FinTrackCore.Domain.Entities;
using FinTrackCore.Domain.Repositories;
using Microsoft.Extensions.Options;
using SharpOutcome;
using SharpOutcome.Helpers;
using SharpOutcome.Helpers.Enums;

namespace FinTrackCore.Application.Features.FinancialYears;

public sealed class FinancialYearService : IFinancialYearService
{
    private readonly IFinancialYearRepository _financialYearRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly MessageSettings _messages;

    public FinancialYearService(
        IFinancialYearRepository financialYearRepository,
        IUnitOfWork unitOfWork,
        IOptions<MessageSettings> messageOptions)
    {
        _financialYearRepository = financialYearRepository;
        _unitOfWork = unitOfWork;
        _messages = messageOptions.Value;
    }

    public async Task<Outcome<IReadOnlyList<FinancialYear>, HttpBadOutcome>> GetAllAsync(
        long userInfoId,
        CancellationToken ct)
    {
        await SyncFinancialYearsAsync(userInfoId, ct);

        var currentYear = DateTime.UtcNow.Year;
        var minVisibleYear = GetMinVisibleYear(currentYear);

        var years = (await _financialYearRepository.GetAllForUserAsync(userInfoId, ct))
            .Where(x => x.Year >= minVisibleYear && x.Year <= currentYear)
            .OrderByDescending(x => x.Year)
            .ToList();

        return years;
    }

    public async Task<Outcome<FinancialYear, HttpBadOutcome>> GetByIdAsync(
        long id,
        long userInfoId,
        CancellationToken ct)
    {
        await SyncFinancialYearsAsync(userInfoId, ct);

        var financialYear = await _financialYearRepository.GetByIdForUserAsync(id, userInfoId, ct);

        if (!IsYearVisible(financialYear.Year))
        {
            return new HttpBadOutcome(HttpBadOutcomeTag.NotFound, _messages.NotFound);
        }

        return financialYear;
    }

    public async Task<Outcome<FinancialYear, HttpBadOutcome>> GetCurrentAsync(
        long userInfoId,
        CancellationToken ct)
    {
        await SyncFinancialYearsAsync(userInfoId, ct);

        var currentYear = DateTime.UtcNow.Year;
        var financialYear = await _financialYearRepository.GetByYearForUserAsync(currentYear, userInfoId, ct);

        if (financialYear is null)
        {
            return new HttpBadOutcome(HttpBadOutcomeTag.NotFound, _messages.NotFound);
        }

        return financialYear;
    }

    private async Task SyncFinancialYearsAsync(long userInfoId, CancellationToken ct)
    {
        await _unitOfWork.ExecuteInTransactionAsync(async innerCt =>
        {
            var currentYear = DateTime.UtcNow.Year;
            var now = DateTime.UtcNow;
            var hasChanges = false;

            if (!await _financialYearRepository.ExistsForUserAndYearAsync(currentYear, userInfoId, innerCt))
            {
                await _unitOfWork.AddAsync(
                    DefaultFinancialYearSeedService.CreateFinancialYear(
                        userInfoId,
                        currentYear,
                        isActive: true,
                        isClosed: false,
                        now),
                    innerCt);
                hasChanges = true;
            }

            var trackedYears = await _financialYearRepository.GetTrackedAllForUserAsync(userInfoId, innerCt);

            foreach (var financialYear in trackedYears)
            {
                if (financialYear.Year > currentYear)
                {
                    continue;
                }

                if (financialYear.Year == currentYear)
                {
                    if (!financialYear.IsActive || financialYear.IsClosed)
                    {
                        financialYear.IsActive = true;
                        financialYear.IsClosed = false;
                        financialYear.UpdatedDate = now;
                        _unitOfWork.Update(financialYear);
                        hasChanges = true;
                    }

                    continue;
                }

                if (financialYear.IsActive || !financialYear.IsClosed)
                {
                    financialYear.IsActive = false;
                    financialYear.IsClosed = true;
                    financialYear.UpdatedDate = now;
                    _unitOfWork.Update(financialYear);
                    hasChanges = true;
                }
            }

            if (hasChanges)
            {
                await _unitOfWork.SaveChangesAsync(innerCt);
            }
        }, ct);
    }

    private static int GetMinVisibleYear(int currentYear)
    {
        return currentYear - FinancialYearConstants.MaxVisibleYears + 1;
    }

    private static bool IsYearVisible(int year)
    {
        var currentYear = DateTime.UtcNow.Year;

        if (year > currentYear)
        {
            return false;
        }

        return year >= GetMinVisibleYear(currentYear);
    }
}
