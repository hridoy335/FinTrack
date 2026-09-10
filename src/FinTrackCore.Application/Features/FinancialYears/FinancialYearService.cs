using FinTrackCore.Application.Common.Configuration;
using FinTrackCore.Application.Common.Models;
using FinTrackCore.Application.Constants;
using FinTrackCore.Application.Features.FinancialYears.Models;
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

    public async Task<Outcome<IReadOnlyList<FinancialYearListItem>, HttpBadOutcome>> GetAllAsync(
        long userInfoId,
        CancellationToken ct)
    {
        await SyncFinancialYearsAsync(userInfoId, ct);

        var currentYear = DateTime.UtcNow.Year;
        var years = (await _financialYearRepository.GetAllForUserAsync(userInfoId, ct))
            .OrderByDescending(x => x.Year)
            .Select(x => MapListItem(x, currentYear))
            .ToList();

        return years;
    }

    public async Task<Outcome<FinancialYear, HttpBadOutcome>> GetByIdAsync(
        long id,
        long userInfoId,
        CancellationToken ct)
    {
        await SyncFinancialYearsAsync(userInfoId, ct);
        return await _financialYearRepository.GetByIdForUserAsync(id, userInfoId, ct);
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

    public async Task<Outcome<MutationResult, HttpBadOutcome>> CreateNextAsync(
        long userInfoId,
        CancellationToken ct)
    {
        await SyncFinancialYearsAsync(userInfoId, ct);

        var years = await _financialYearRepository.GetAllForUserAsync(userInfoId, ct);
        var currentYear = DateTime.UtcNow.Year;
        var maxYear = years.Count == 0
            ? currentYear
            : Math.Max(years.Max(x => x.Year), currentYear);

        var nextYear = maxYear + 1;

        if (await _financialYearRepository.ExistsForUserAndYearAsync(nextYear, userInfoId, ct))
        {
            return new HttpBadOutcome(HttpBadOutcomeTag.Conflict, _messages.FinancialYearAlreadyExists);
        }

        var now = DateTime.UtcNow;
        var financialYear = DefaultFinancialYearSeedService.CreateFinancialYear(
            userInfoId,
            nextYear,
            isActive: false,
            isClosed: false,
            now);

        await _unitOfWork.AddAsync(financialYear, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return new MutationResult
        {
            Id = financialYear.Id,
            Message = _messages.FinancialYearCreateNextSuccess
        };
    }

    public async Task<Outcome<MutationResult, HttpBadOutcome>> UpdateAsync(
        long id,
        long userInfoId,
        UpdateFinancialYearRequest request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return new HttpBadOutcome(HttpBadOutcomeTag.BadRequest, _messages.InvalidFinancialYearName);
        }

        var financialYear = await _financialYearRepository.GetByIdForUserAsync(id, userInfoId, ct);
        var currentYear = DateTime.UtcNow.Year;

        if (financialYear.Year == currentYear && request.IsClosed)
        {
            return new HttpBadOutcome(HttpBadOutcomeTag.BadRequest, _messages.CannotCloseCurrentFinancialYear);
        }

        financialYear.Name = request.Name.Trim();
        financialYear.IsClosed = request.IsClosed;
        financialYear.IsActive = financialYear.Year == currentYear && !request.IsClosed;
        financialYear.UpdatedDate = DateTime.UtcNow;

        _unitOfWork.Update(financialYear);
        await _unitOfWork.SaveChangesAsync(ct);

        return new MutationResult
        {
            Id = financialYear.Id,
            Message = _messages.UpdateSuccess
        };
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
                    if (financialYear.IsActive)
                    {
                        financialYear.IsActive = false;
                        financialYear.UpdatedDate = now;
                        _unitOfWork.Update(financialYear);
                        hasChanges = true;
                    }

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

    private static FinancialYearListItem MapListItem(FinancialYear year, int currentYear) =>
        new()
        {
            Id = year.Id,
            Year = year.Year,
            Name = year.Name,
            StartDate = year.StartDate,
            EndDate = year.EndDate,
            IsActive = year.IsActive,
            IsClosed = year.IsClosed,
            IsCurrent = year.Year == currentYear,
            CanEdit = true
        };
}
