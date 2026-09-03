using FinTrackCore.Application.Common.Configuration;
using FinTrackCore.Application.Common.Models;
using FinTrackCore.Application.Constants;
using FinTrackCore.Application.Features.Coas.Models;
using FinTrackCore.Application.Interfaces;
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
    private readonly ICoaListPdfExporter _coaListPdfExporter;
    private readonly MessageSettings _messages;

    public CoaService(
        ICoaRepository coaRepository,
        IAccountTypeRepository accountTypeRepository,
        IUnitOfWork unitOfWork,
        ICoaListPdfExporter coaListPdfExporter,
        IOptions<MessageSettings> messageOptions)
    {
        _coaRepository = coaRepository;
        _accountTypeRepository = accountTypeRepository;
        _unitOfWork = unitOfWork;
        _coaListPdfExporter = coaListPdfExporter;
        _messages = messageOptions.Value;
    }

    public async Task<Outcome<IReadOnlyList<Coa>, HttpBadOutcome>> GetAllAsync(
        long userInfoId,
        CancellationToken ct)
    {
        return (await _coaRepository.GetAllForUserAsync(userInfoId, ct)).ToList();
    }

    public async Task<Outcome<CoaListResponse, HttpBadOutcome>> GetListAsync(
        long userInfoId,
        CancellationToken ct)
    {
        var coas = await _coaRepository.GetAllForUserAsync(userInfoId, ct);
        var usedCoaIds = await _coaRepository.GetCoaIdsUsedInTransactionsAsync(userInfoId, ct);
        return MapToListResponse(coas, usedCoaIds);
    }

    public async Task<Outcome<byte[], HttpBadOutcome>> ExportListPdfAsync(
        long userInfoId,
        string userDisplayName,
        CancellationToken ct)
    {
        var listResult = await GetListAsync(userInfoId, ct);

        if (listResult.TryPickBadOutcome(out var error))
        {
            return error;
        }

        listResult.TryPickGoodOutcome(out var list);
        var pdfBytes = _coaListPdfExporter.Generate(list!, userDisplayName);

        return pdfBytes;
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
        if (string.IsNullOrWhiteSpace(request.AccountName))
        {
            return new HttpBadOutcome(HttpBadOutcomeTag.BadRequest, _messages.ValidationFailed);
        }

        if (!CoaConstants.IsValidAccountTypeId(request.AccountTypeId))
        {
            return new HttpBadOutcome(HttpBadOutcomeTag.BadRequest, _messages.InvalidAccountType);
        }

        _ = await _accountTypeRepository.GetByIdAsync(request.AccountTypeId, ct);

        var accountName = request.AccountName.Trim();
        if (await _coaRepository.ExistsByAccountNameForUserAndAccountTypeAsync(
                userInfoId,
                request.AccountTypeId,
                accountName,
                excludeCoaId: null,
                ct))
        {
            return new HttpBadOutcome(HttpBadOutcomeTag.Conflict, _messages.DuplicateAccountHeadName);
        }

        var existingCodes = await _coaRepository.GetAccountCodesForUserAndAccountTypeAsync(
            userInfoId,
            request.AccountTypeId,
            ct);
        var code = CoaConstants.GetNextAccountCode(request.AccountTypeId, existingCodes);

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
            AccountName = accountName,
            IsSystemDefault = false,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        await _unitOfWork.AddAsync(coa, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return new MutationResult
        {
            Id = coa.Id,
            Message = _messages.InsertSuccess,
            AccountCode = coa.AccountCode
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

        if (coa.IsSystemDefault)
        {
            return new HttpBadOutcome(HttpBadOutcomeTag.Forbidden, _messages.SystemAccountUpdateForbidden);
        }

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

        var accountName = request.AccountName.Trim();
        if (await _coaRepository.ExistsByAccountNameForUserAndAccountTypeAsync(
                userInfoId,
                coa.AccountTypeId,
                accountName,
                excludeCoaId: id,
                ct))
        {
            return new HttpBadOutcome(HttpBadOutcomeTag.Conflict, _messages.DuplicateAccountHeadName);
        }

        coa.ParentId = request.ParentId;
        coa.AccountName = accountName;
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
            return new HttpBadOutcome(HttpBadOutcomeTag.Conflict, _messages.CoaHasChildrenDeleteForbidden);
        }

        if (await _coaRepository.IsUsedInTransactionsAsync(id, userInfoId, ct))
        {
            return new HttpBadOutcome(HttpBadOutcomeTag.Conflict, _messages.CoaInUseDeleteForbidden);
        }

        _unitOfWork.Remove(coa);
        await _unitOfWork.SaveChangesAsync(ct);

        return new MutationResult
        {
            Id = id,
            Message = _messages.DeleteSuccess
        };
    }

    private static CoaListResponse MapToListResponse(
        IReadOnlyList<Coa> coas,
        IReadOnlySet<long> usedCoaIds)
    {
        var sections = coas
            .GroupBy(x => new
            {
                x.AccountTypeId,
                AccountTypeCode = x.AccountType!.Code,
                AccountTypeName = x.AccountType.Name
            })
            .OrderBy(x => x.Key.AccountTypeId)
            .Select(group => new CoaListSectionResponse
            {
                AccountTypeId = group.Key.AccountTypeId,
                AccountTypeCode = group.Key.AccountTypeCode,
                AccountTypeName = group.Key.AccountTypeName,
                Items = group
                    .OrderBy(x => x.AccountCode)
                    .Select(x => new CoaListItemResponse
                    {
                        Id = x.Id,
                        Code = x.AccountCode,
                        AccountHeadName = x.AccountName,
                        ParentId = x.ParentId,
                        IsSystemDefault = x.IsSystemDefault,
                        IsActive = x.IsActive,
                        CanEdit = !x.IsSystemDefault,
                        CanDelete = !x.IsSystemDefault && !usedCoaIds.Contains(x.Id)
                    })
                    .ToList()
            })
            .ToList();

        return new CoaListResponse { Sections = sections };
    }
}
