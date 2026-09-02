using FinTrackCore.Application.Constants;
using FinTrackCore.Domain;
using FinTrackCore.Domain.Entities;
using FinTrackCore.Domain.Repositories;

namespace FinTrackCore.Application.Features.Coas;

public sealed class DefaultCoaSeedService : IDefaultCoaSeedService
{
    private readonly IUnitOfWork _unitOfWork;

    public DefaultCoaSeedService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task SeedForUserAsync(long userInfoId, CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        var assets = Create(userInfoId, AccountTypeIds.Asset, DefaultCoaConstants.Codes.Assets, DefaultCoaConstants.Names.Assets, null, now);
        var cash = Create(userInfoId, AccountTypeIds.Asset, DefaultCoaConstants.Codes.Cash, DefaultCoaConstants.Names.Cash, assets, now);
        var bank = Create(userInfoId, AccountTypeIds.Asset, DefaultCoaConstants.Codes.Bank, DefaultCoaConstants.Names.Bank, assets, now);
        var wallet = Create(userInfoId, AccountTypeIds.Asset, DefaultCoaConstants.Codes.MobileWallet, DefaultCoaConstants.Names.MobileWallet, assets, now);
        var savings = Create(userInfoId, AccountTypeIds.Asset, DefaultCoaConstants.Codes.Savings, DefaultCoaConstants.Names.Savings, assets, now);
        var loanReceivable = Create(userInfoId, AccountTypeIds.Asset, DefaultCoaConstants.Codes.LoanReceivable, DefaultCoaConstants.Names.LoanReceivable, assets, now);

        var liabilities = Create(userInfoId, AccountTypeIds.Liability, DefaultCoaConstants.Codes.Liabilities, DefaultCoaConstants.Names.Liabilities, null, now);
        var creditCard = Create(userInfoId, AccountTypeIds.Liability, DefaultCoaConstants.Codes.CreditCard, DefaultCoaConstants.Names.CreditCard, liabilities, now);
        var personalLoan = Create(userInfoId, AccountTypeIds.Liability, DefaultCoaConstants.Codes.PersonalLoan, DefaultCoaConstants.Names.PersonalLoan, liabilities, now);

        var equity = Create(userInfoId, AccountTypeIds.Equity, DefaultCoaConstants.Codes.Equity, DefaultCoaConstants.Names.Equity, null, now);
        var opening = Create(userInfoId, AccountTypeIds.Equity, DefaultCoaConstants.Codes.OpeningBalance, DefaultCoaConstants.Names.OpeningBalance, equity, now);

        var income = Create(userInfoId, AccountTypeIds.Income, DefaultCoaConstants.Codes.Income, DefaultCoaConstants.Names.Income, null, now);
        var salary = Create(userInfoId, AccountTypeIds.Income, DefaultCoaConstants.Codes.Salary, DefaultCoaConstants.Names.Salary, income, now);
        var freelance = Create(userInfoId, AccountTypeIds.Income, DefaultCoaConstants.Codes.FreelanceIncome, DefaultCoaConstants.Names.FreelanceIncome, income, now);
        var business = Create(userInfoId, AccountTypeIds.Income, DefaultCoaConstants.Codes.BusinessIncome, DefaultCoaConstants.Names.BusinessIncome, income, now);
        var investment = Create(userInfoId, AccountTypeIds.Income, DefaultCoaConstants.Codes.InvestmentIncome, DefaultCoaConstants.Names.InvestmentIncome, income, now);

        var expenses = Create(userInfoId, AccountTypeIds.Expense, DefaultCoaConstants.Codes.Expenses, DefaultCoaConstants.Names.Expenses, null, now);
        var food = Create(userInfoId, AccountTypeIds.Expense, DefaultCoaConstants.Codes.Food, DefaultCoaConstants.Names.Food, expenses, now);
        var transport = Create(userInfoId, AccountTypeIds.Expense, DefaultCoaConstants.Codes.Transport, DefaultCoaConstants.Names.Transport, expenses, now);
        var rent = Create(userInfoId, AccountTypeIds.Expense, DefaultCoaConstants.Codes.Rent, DefaultCoaConstants.Names.Rent, expenses, now);
        var utilities = Create(userInfoId, AccountTypeIds.Expense, DefaultCoaConstants.Codes.Utilities, DefaultCoaConstants.Names.Utilities, expenses, now);
        var shopping = Create(userInfoId, AccountTypeIds.Expense, DefaultCoaConstants.Codes.Shopping, DefaultCoaConstants.Names.Shopping, expenses, now);
        var entertainment = Create(userInfoId, AccountTypeIds.Expense, DefaultCoaConstants.Codes.Entertainment, DefaultCoaConstants.Names.Entertainment, expenses, now);
        var health = Create(userInfoId, AccountTypeIds.Expense, DefaultCoaConstants.Codes.Health, DefaultCoaConstants.Names.Health, expenses, now);
        var education = Create(userInfoId, AccountTypeIds.Expense, DefaultCoaConstants.Codes.Education, DefaultCoaConstants.Names.Education, expenses, now);
        var insurance = Create(userInfoId, AccountTypeIds.Expense, DefaultCoaConstants.Codes.Insurance, DefaultCoaConstants.Names.Insurance, expenses, now);

        Coa[] accounts =
        [
            assets, cash, bank, wallet, savings, loanReceivable,
            liabilities, creditCard, personalLoan,
            equity, opening,
            income, salary, freelance, business, investment,
            expenses, food, transport, rent, utilities, shopping, entertainment, health, education, insurance
        ];

        foreach (var account in accounts)
        {
            await _unitOfWork.AddAsync(account, ct);
        }

        await _unitOfWork.SaveChangesAsync(ct);
    }

    private static Coa Create(
        long userInfoId,
        long accountTypeId,
        string code,
        string name,
        Coa? parent,
        DateTime createdDate)
    {
        return new Coa
        {
            UserInfoId = userInfoId,
            AccountTypeId = accountTypeId,
            AccountCode = code,
            AccountName = name,
            Parent = parent,
            IsSystemDefault = true,
            IsActive = true,
            CreatedDate = createdDate
        };
    }
}
