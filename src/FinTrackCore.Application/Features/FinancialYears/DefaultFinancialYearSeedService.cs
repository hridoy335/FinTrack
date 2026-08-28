using FinTrackCore.Application.Constants;
using FinTrackCore.Domain.Entities;
using FinTrackCore.Domain.Repositories;

namespace FinTrackCore.Application.Features.FinancialYears;

public sealed class DefaultFinancialYearSeedService : IDefaultFinancialYearSeedService
{
    private readonly IFinancialYearRepository _financialYearRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DefaultFinancialYearSeedService(
        IFinancialYearRepository financialYearRepository,
        IUnitOfWork unitOfWork)
    {
        _financialYearRepository = financialYearRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task SeedForUserAsync(long userInfoId, CancellationToken ct)
    {
        var currentYear = DateTime.UtcNow.Year;

        if (await _financialYearRepository.ExistsForUserAndYearAsync(currentYear, userInfoId, ct))
        {
            return;
        }

        var now = DateTime.UtcNow;
        var financialYear = CreateFinancialYear(userInfoId, currentYear, isActive: true, isClosed: false, now);

        await _unitOfWork.AddAsync(financialYear, ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    internal static FinancialYear CreateFinancialYear(
        long userInfoId,
        int year,
        bool isActive,
        bool isClosed,
        DateTime createdDate)
    {
        return new FinancialYear
        {
            UserInfoId = userInfoId,
            Year = year,
            Name = string.Format(FinancialYearConstants.Names.Format, year),
            StartDate = new DateTime(
                year,
                FinancialYearConstants.CalendarStartMonth,
                FinancialYearConstants.CalendarStartDay,
                0,
                0,
                0,
                DateTimeKind.Utc),
            EndDate = new DateTime(
                year,
                FinancialYearConstants.CalendarEndMonth,
                FinancialYearConstants.CalendarEndDay,
                0,
                0,
                0,
                DateTimeKind.Utc),
            IsActive = isActive,
            IsClosed = isClosed,
            CreatedDate = createdDate
        };
    }
}
