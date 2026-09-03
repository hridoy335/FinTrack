using FinTrackCore.Application.Common.Configuration;
using FinTrackCore.Application.Common.Models;
using FinTrackCore.Application.Constants;
using FinTrackCore.Application.Features.Coas;
using FinTrackCore.Application.Features.FinancialYears;
using FinTrackCore.Application.Features.UserInfos.Models;
using FinTrackCore.Application.Interfaces;
using FinTrackCore.Domain.Entities;
using FinTrackCore.Domain.Repositories;
using Microsoft.Extensions.Options;
using SharpOutcome;
using SharpOutcome.Helpers;
using SharpOutcome.Helpers.Enums;

namespace FinTrackCore.Application.Features.UserInfos;

public class UserInfoService : IUserInfoService
{
    private readonly IUserInfoRepository _userInfoRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordService _passwordService;
    private readonly IDefaultCoaSeedService _defaultCoaSeedService;
    private readonly IDefaultFinancialYearSeedService _defaultFinancialYearSeedService;
    private readonly MessageSettings _messages;

    public UserInfoService(
        IUserInfoRepository userInfoRepository,
        IUnitOfWork unitOfWork,
        IPasswordService passwordService,
        IDefaultCoaSeedService defaultCoaSeedService,
        IDefaultFinancialYearSeedService defaultFinancialYearSeedService,
        IOptions<MessageSettings> messageOptions)
    {
        _userInfoRepository = userInfoRepository;
        _unitOfWork = unitOfWork;
        _passwordService = passwordService;
        _defaultCoaSeedService = defaultCoaSeedService;
        _defaultFinancialYearSeedService = defaultFinancialYearSeedService;
        _messages = messageOptions.Value;
    }

    public async Task<Outcome<MutationResult, HttpBadOutcome>> CreateAsync(
        CreateUserInfoRequest request,
        CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (await _userInfoRepository.GetByEmailAsync(email, ct) is not null)
        {
            return new HttpBadOutcome(HttpBadOutcomeTag.Conflict, _messages.DuplicateEmail);
        }

        var userInfo = new UserInfo
        {
            Email = email,
            PasswordHash = _passwordService.Hash(request.Password),
            FirstName = request.FirstName.Trim(),
            LastName = string.IsNullOrWhiteSpace(request.LastName) ? null : request.LastName.Trim(),
            CurrencyCode = string.IsNullOrWhiteSpace(request.CurrencyCode)
                ? CurrencyConstants.DefaultCurrencyCode
                : request.CurrencyCode.Trim().ToUpperInvariant(),
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        await _unitOfWork.ExecuteInTransactionAsync(async innerCt =>
        {
            await _unitOfWork.AddAsync(userInfo, innerCt);
            await _unitOfWork.SaveChangesAsync(innerCt);
            await _defaultCoaSeedService.SeedForUserAsync(userInfo.Id, innerCt);
            await _defaultFinancialYearSeedService.SeedForUserAsync(userInfo.Id, innerCt);
        }, ct);

        return new MutationResult
        {
            Id = userInfo.Id,
            Message = _messages.InsertSuccess
        };
    }

    public async Task<Outcome<MutationResult, HttpBadOutcome>> UpdateAsync(
        long id,
        UpdateUserInfoRequest request,
        CancellationToken ct)
    {
        var userInfo = await _userInfoRepository.GetByIdAsync(id, ct);

        var email = request.Email.Trim().ToLowerInvariant();

        var existingByEmail = await _userInfoRepository.GetByEmailAsync(email, ct);
        if (existingByEmail is not null && existingByEmail.Id != id)
        {
            return new HttpBadOutcome(HttpBadOutcomeTag.Conflict, _messages.DuplicateEmail);
        }

        userInfo.Email = email;
        userInfo.FirstName = request.FirstName.Trim();
        userInfo.LastName = string.IsNullOrWhiteSpace(request.LastName) ? null : request.LastName.Trim();
        userInfo.CurrencyCode = string.IsNullOrWhiteSpace(request.CurrencyCode)
            ? CurrencyConstants.DefaultCurrencyCode
            : request.CurrencyCode.Trim().ToUpperInvariant();
        userInfo.IsActive = request.IsActive;
        userInfo.UpdatedDate = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            userInfo.PasswordHash = _passwordService.Hash(request.Password);
        }

        _unitOfWork.Update(userInfo);
        await _unitOfWork.SaveChangesAsync(ct);

        return new MutationResult
        {
            Id = userInfo.Id,
            Message = _messages.UpdateSuccess
        };
    }

    public async Task<Outcome<UserInfoResponse, HttpBadOutcome>> GetByIdAsync(
        long id,
        CancellationToken ct)
    {
        var userInfo = await _userInfoRepository.GetByIdAsync(id, ct);

        return new UserInfoResponse
        {
            Id = userInfo.Id,
            Email = userInfo.Email,
            FirstName = userInfo.FirstName,
            LastName = userInfo.LastName,
            CurrencyCode = userInfo.CurrencyCode,
            IsActive = userInfo.IsActive,
            CreatedDate = userInfo.CreatedDate,
            UpdatedDate = userInfo.UpdatedDate
        };
    }
}
