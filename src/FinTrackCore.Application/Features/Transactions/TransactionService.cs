using FinTrackCore.Application.Common.Configuration;
using FinTrackCore.Application.Common.Models;
using FinTrackCore.Application.Constants;
using FinTrackCore.Application.Features.Transactions.Models;
using FinTrackCore.Domain;
using FinTrackCore.Domain.Entities;
using FinTrackCore.Domain.Repositories;
using Microsoft.Extensions.Options;
using SharpOutcome;
using SharpOutcome.Helpers;
using SharpOutcome.Helpers.Enums;

namespace FinTrackCore.Application.Features.Transactions;

public sealed class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IFinancialYearRepository _financialYearRepository;
    private readonly ITransactionTypeRepository _transactionTypeRepository;
    private readonly ICoaRepository _coaRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly MessageSettings _messages;

    public TransactionService(
        ITransactionRepository transactionRepository,
        IFinancialYearRepository financialYearRepository,
        ITransactionTypeRepository transactionTypeRepository,
        ICoaRepository coaRepository,
        IUnitOfWork unitOfWork,
        IOptions<MessageSettings> messageOptions)
    {
        _transactionRepository = transactionRepository;
        _financialYearRepository = financialYearRepository;
        _transactionTypeRepository = transactionTypeRepository;
        _coaRepository = coaRepository;
        _unitOfWork = unitOfWork;
        _messages = messageOptions.Value;
    }

    public async Task<Outcome<(IReadOnlyList<Transaction> Items, long TotalCount), HttpBadOutcome>> GetAllAsync(
        long userInfoId,
        TransactionListQuery query,
        CancellationToken ct)
    {
        var page = query.Page < PaginationConstants.MinPageSize
            ? PaginationConstants.MinPageSize
            : query.Page;

        var pageSize = query.PageSize <= 0
            ? PaginationConstants.DefaultPageSize
            : Math.Min(query.PageSize, PaginationConstants.MaxPageSize);

        DateTime? fromDate = query.FromDate.HasValue ? ToUtcDate(query.FromDate.Value) : null;
        DateTime? toDate = query.ToDate.HasValue ? ToUtcDate(query.ToDate.Value) : null;

        var result = await _transactionRepository.GetPagedForUserAsync(
            userInfoId,
            query.FinancialYearId,
            query.TransactionTypeId,
            fromDate,
            toDate,
            page,
            pageSize,
            ct);

        return result;
    }

    public async Task<Outcome<Transaction, HttpBadOutcome>> GetByIdAsync(
        long id,
        long userInfoId,
        CancellationToken ct)
    {
        return await _transactionRepository.GetByIdForUserAsync(id, userInfoId, ct);
    }

    public async Task<Outcome<MutationResult, HttpBadOutcome>> CreateAsync(
        long userInfoId,
        CreateTransactionRequest request,
        CancellationToken ct)
    {
        if (request.Amount < TransactionConstants.MinAmount)
        {
            return new HttpBadOutcome(HttpBadOutcomeTag.BadRequest, _messages.InvalidAmount);
        }

        if (request.DebitCoaId == request.CreditCoaId)
        {
            return new HttpBadOutcome(HttpBadOutcomeTag.BadRequest, _messages.SameDebitCreditCoa);
        }

        if (!await _transactionTypeRepository.ExistsAsync(request.TransactionTypeId, ct))
        {
            return new HttpBadOutcome(HttpBadOutcomeTag.BadRequest, _messages.InvalidTransactionType);
        }

        var financialYear = await _financialYearRepository.GetByIdForUserAsync(
            request.FinancialYearId,
            userInfoId,
            ct);

        if (!financialYear.IsActive || financialYear.IsClosed)
        {
            return new HttpBadOutcome(HttpBadOutcomeTag.BadRequest, _messages.FinancialYearClosed);
        }

        var calendarDate = request.TransactionDate.Date;
        var transactionDate = ToUtcDate(calendarDate);

        if (transactionDate < financialYear.StartDate || transactionDate > financialYear.EndDate)
        {
            return new HttpBadOutcome(HttpBadOutcomeTag.BadRequest, _messages.TransactionDateOutOfRange);
        }

        var debitCoa = await _coaRepository.GetByIdForUserAsync(request.DebitCoaId, userInfoId, ct);
        var creditCoa = await _coaRepository.GetByIdForUserAsync(request.CreditCoaId, userInfoId, ct);

        if (!debitCoa.IsActive || !creditCoa.IsActive)
        {
            return new HttpBadOutcome(HttpBadOutcomeTag.BadRequest, _messages.InvalidCoa);
        }

        if (!IsValidCoaPairForTransactionType(
                request.TransactionTypeId,
                debitCoa.AccountTypeId,
                creditCoa.AccountTypeId))
        {
            return new HttpBadOutcome(HttpBadOutcomeTag.BadRequest, _messages.InvalidCoaForTransactionType);
        }

        var now = DateTime.UtcNow;
        var amount = decimal.Round(request.Amount, TransactionConstants.AmountScale);

        var transaction = new Transaction
        {
            UserInfoId = userInfoId,
            FinancialYearId = request.FinancialYearId,
            TransactionTypeId = request.TransactionTypeId,
            TransactionDate = transactionDate,
            Amount = amount,
            Description = string.IsNullOrWhiteSpace(request.Description)
                ? null
                : request.Description.Trim(),
            CreatedDate = now,
            VoucherLines =
            [
                new VoucherLine
                {
                    CoaId = debitCoa.Id,
                    LineNumber = 1,
                    DebitAmount = amount,
                    CreditAmount = 0,
                    CreatedDate = now
                },
                new VoucherLine
                {
                    CoaId = creditCoa.Id,
                    LineNumber = 2,
                    DebitAmount = 0,
                    CreditAmount = amount,
                    CreatedDate = now
                }
            ]
        };

        await _unitOfWork.ExecuteInTransactionAsync(async innerCt =>
        {
            await _unitOfWork.AddAsync(transaction, innerCt);
            await _unitOfWork.SaveChangesAsync(innerCt);
        }, ct);

        return new MutationResult
        {
            Id = transaction.Id,
            Message = _messages.InsertSuccess
        };
    }

    private static DateTime ToUtcDate(DateTime value) =>
        new(value.Year, value.Month, value.Day, 0, 0, 0, DateTimeKind.Utc);

    private static bool IsValidCoaPairForTransactionType(
        long transactionTypeId,
        long debitAccountTypeId,
        long creditAccountTypeId)
    {
        return transactionTypeId switch
        {
            TransactionTypeIds.Income => debitAccountTypeId == AccountTypeIds.Asset
                && creditAccountTypeId == AccountTypeIds.Income,
            TransactionTypeIds.Expense => debitAccountTypeId == AccountTypeIds.Expense
                && creditAccountTypeId == AccountTypeIds.Asset,
            TransactionTypeIds.Transfer => debitAccountTypeId == AccountTypeIds.Asset
                && creditAccountTypeId == AccountTypeIds.Asset,
            TransactionTypeIds.OpeningBalance => debitAccountTypeId == AccountTypeIds.Asset
                && creditAccountTypeId == AccountTypeIds.Equity,
            TransactionTypeIds.LoanBorrow => debitAccountTypeId == AccountTypeIds.Asset
                && creditAccountTypeId == AccountTypeIds.Liability,
            TransactionTypeIds.LoanRepay => debitAccountTypeId == AccountTypeIds.Liability
                && creditAccountTypeId == AccountTypeIds.Asset,
            _ => false
        };
    }
}
