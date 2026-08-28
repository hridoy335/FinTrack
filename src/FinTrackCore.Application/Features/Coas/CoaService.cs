using FinTrackCore.Application.Common.Configuration;
using FinTrackCore.Application.Common.Models;
using FinTrackCore.Application.Features.Coas.Models;
using FinTrackCore.Domain.Entities;
using FinTrackCore.Domain.Repositories;
using Microsoft.Extensions.Options;
using SharpOutcome;
using SharpOutcome.Helpers;
using SharpOutcome.Helpers.Enums;

namespace FinTrackCore.Application.Features.Coas;

public sealed class CoaService : ICoaService
{
    private readonly ICoaRepository _coaRepository;
    private readonly IAccountTypeRepository _accountTypeRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly MessageSettings _messages;

    public CoaService(
        ICoaRepository coaRepository,
        IAccountTypeRepository accountTypeRepository,
        IUnitOfWork unitOfWork,
        IOptions<MessageSettings> messageOptions)
    {
        _coaRepository = coaRepository;
        _accountTypeRepository = accountTypeRepository;
        _unitOfWork = unitOfWork;
        _messages = messageOptions.Value;
    }

    public async Task<Outcome<IReadOnlyList<Coa>, HttpBadOutcome>> GetAllAsync(
        long userInfoId,
        CancellationToken ct)
    {
        return (await _coaRepository.GetAllForUserAsync(userInfoId, ct)).ToList();
    }

    public async Task<Outcome<Coa, HttpBadOutcome>> GetByIdAsync(
        long id,
        long userInfoId,
        CancellationToken ct)
    {
        return await _coaRepository.GetByIdForUserAsync(id, userInfoId, ct);
    }

    public async Task<Outcome<MutationResult, HttpBadOutcome>> CreateAsync(
        long userInfoId,
        CreateCoaRequest request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.AccountCode) || string.IsNullOrWhiteSpace(request.AccountName))
        {
            return new HttpBadOutcome(HttpBadOutcomeTag.BadRequest, _messages.ValidationFailed);
        }

        _ = await _accountTypeRepository.GetByIdAsync(request.AccountTypeId, ct);

        var code = request.AccountCode.Trim();
        if (await _coaRepository.ExistsByCodeForUserAsync(code, userInfoId, ct))
        {
            return new HttpBadOutcome(HttpBadOutcomeTag.Conflict, _messages.Conflict);
        }

        if (request.ParentId is not null)
        {
            var parent = await _coaRepository.GetByIdForUserAsync(request.ParentId.Value, userInfoId, ct);
            if (parent.AccountTypeId != request.AccountTypeId)
            {
                return new HttpBadOutcome(HttpBadOutcomeTag.BadRequest, _messages.InvalidParentAccount);
            }
        }

        var coa = new Coa
        {
            UserInfoId = userInfoId,
            ParentId = request.ParentId,
            AccountTypeId = request.AccountTypeId,
            AccountCode = code,
            AccountName = request.AccountName.Trim(),
            IsSystemDefault = false,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        await _unitOfWork.AddAsync(coa, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return new MutationResult
        {
            Id = coa.Id,
            Message = _messages.InsertSuccess
        };
    }

    public async Task<Outcome<MutationResult, HttpBadOutcome>> UpdateAsync(
        long id,
        long userInfoId,
        UpdateCoaRequest request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.AccountName))
        {
            return new HttpBadOutcome(HttpBadOutcomeTag.BadRequest, _messages.ValidationFailed);
        }

        var coa = await _coaRepository.GetByIdForUserAsync(id, userInfoId, ct);

        if (request.ParentId == id)
        {
            return new HttpBadOutcome(HttpBadOutcomeTag.BadRequest, _messages.InvalidParentAccount);
        }

        if (request.ParentId is not null)
        {
            var parent = await _coaRepository.GetByIdForUserAsync(request.ParentId.Value, userInfoId, ct);
            if (parent.AccountTypeId != coa.AccountTypeId)
            {
                return new HttpBadOutcome(HttpBadOutcomeTag.BadRequest, _messages.InvalidParentAccount);
            }
        }

        coa.ParentId = request.ParentId;
        coa.AccountName = request.AccountName.Trim();
        coa.IsActive = request.IsActive;
        coa.UpdatedDate = DateTime.UtcNow;

        _unitOfWork.Update(coa);
        await _unitOfWork.SaveChangesAsync(ct);

        return new MutationResult
        {
            Id = coa.Id,
            Message = _messages.UpdateSuccess
        };
    }

    public async Task<Outcome<MutationResult, HttpBadOutcome>> DeleteAsync(
        long id,
        long userInfoId,
        CancellationToken ct)
    {
        var coa = await _coaRepository.GetByIdForUserAsync(id, userInfoId, ct);

        if (coa.IsSystemDefault)
        {
            return new HttpBadOutcome(HttpBadOutcomeTag.Forbidden, _messages.SystemAccountDeleteForbidden);
        }

        if (await _coaRepository.HasChildrenAsync(id, userInfoId, ct))
        {
            return new HttpBadOutcome(HttpBadOutcomeTag.Conflict, _messages.Conflict);
        }

        _unitOfWork.Remove(coa);
        await _unitOfWork.SaveChangesAsync(ct);

        return new MutationResult
        {
            Id = id,
            Message = _messages.DeleteSuccess
        };
    }
}
