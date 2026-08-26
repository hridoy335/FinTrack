using FinTrackCore.Application.Common.Configuration;
using FinTrackCore.Application.Common.Models;
using FinTrackCore.Application.Features.Users.Models;
using FinTrackCore.Application.Interfaces;
using FinTrackCore.Domain.Entities;
using FinTrackCore.Domain.Repositories;
using Microsoft.Extensions.Options;
using SharpOutcome;
using SharpOutcome.Helpers;
using SharpOutcome.Helpers.Enums;

namespace FinTrackCore.Application.Features.Users;

public class UserInfoService : IUserInfoService
{
    private readonly IUserInfoRepository _userInfoRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordService _passwordService;
    private readonly MessageSettings _messages;

    public UserInfoService(
        IUserInfoRepository userInfoRepository,
        IUnitOfWork unitOfWork,
        IPasswordService passwordService,
        IOptions<MessageSettings> messageOptions)
    {
        _userInfoRepository = userInfoRepository;
        _unitOfWork = unitOfWork;
        _passwordService = passwordService;
        _messages = messageOptions.Value;
    }

    public async Task<Outcome<MutationResult, HttpBadOutcome>> CreateAsync(
        CreateUserInfoRequest request,
        CancellationToken cancellationToken = default)  
    {
        var userName = request.UserName.Trim();
        var email = request.Email.Trim().ToLowerInvariant();

        if (await _userInfoRepository.GetByUserNameAsync(userName, cancellationToken) is not null)
        {
            return new HttpBadOutcome(HttpBadOutcomeTag.Conflict, _messages.Conflict);
        }

        if (await _userInfoRepository.GetByEmailAsync(email, cancellationToken) is not null)
        {
            return new HttpBadOutcome(HttpBadOutcomeTag.Conflict, _messages.Conflict);
        }

        var userInfo = new UserInfo
        {
            UserName = userName,
            Email = email,
            PasswordHash = _passwordService.Hash(request.Password),
            FirstName = request.FirstName.Trim(),
            LastName = string.IsNullOrWhiteSpace(request.LastName) ? null : request.LastName.Trim(),
            CurrencyCode = string.IsNullOrWhiteSpace(request.CurrencyCode)
                ? "BDT"
                : request.CurrencyCode.Trim().ToUpperInvariant(),
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        await _unitOfWork.AddAsync(userInfo, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new MutationResult
        {
            Id = userInfo.Id,
            Message = _messages.InsertSuccess
        };
    }

    public async Task<Outcome<MutationResult, HttpBadOutcome>> UpdateAsync(
        long id,
        UpdateUserInfoRequest request,
        CancellationToken cancellationToken = default)
    {
        var userInfo = await _userInfoRepository.GetByIdAsync(id, cancellationToken);
        if (userInfo is null)
        {
            return new HttpBadOutcome(HttpBadOutcomeTag.NotFound, _messages.NotFound);
        }

        var userName = request.UserName.Trim();
        var email = request.Email.Trim().ToLowerInvariant();

        var existingByUserName = await _userInfoRepository.GetByUserNameAsync(userName, cancellationToken);
        if (existingByUserName is not null && existingByUserName.Id != id)
        {
            return new HttpBadOutcome(HttpBadOutcomeTag.Conflict, _messages.Conflict);
        }

        var existingByEmail = await _userInfoRepository.GetByEmailAsync(email, cancellationToken);
        if (existingByEmail is not null && existingByEmail.Id != id)
        {
            return new HttpBadOutcome(HttpBadOutcomeTag.Conflict, _messages.Conflict);
        }

        userInfo.UserName = userName;
        userInfo.Email = email;
        userInfo.FirstName = request.FirstName.Trim();
        userInfo.LastName = string.IsNullOrWhiteSpace(request.LastName) ? null : request.LastName.Trim();
        userInfo.CurrencyCode = string.IsNullOrWhiteSpace(request.CurrencyCode)
            ? "BDT"
            : request.CurrencyCode.Trim().ToUpperInvariant();
        userInfo.IsActive = request.IsActive;
        userInfo.UpdatedDate = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            userInfo.PasswordHash = _passwordService.Hash(request.Password);
        }

        _unitOfWork.Update(userInfo);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new MutationResult
        {
            Id = userInfo.Id,
            Message = _messages.UpdateSuccess
        };
    }

    public async Task<Outcome<UserInfoResponse, HttpBadOutcome>> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        var userInfo = await _userInfoRepository.GetByIdAsync(id, cancellationToken);
        if (userInfo is null)
        {
            return new HttpBadOutcome(HttpBadOutcomeTag.NotFound, _messages.NotFound);
        }

        return new UserInfoResponse
        {
            Id = userInfo.Id,
            UserName = userInfo.UserName,
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
