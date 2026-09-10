using FinTrackCore.Application.Common.Configuration;
using FinTrackCore.Application.Features.AccountTypes;
using FinTrackCore.Application.Features.Auth;
using FinTrackCore.Application.Features.Coas;
using FinTrackCore.Application.Features.FinancialYears;
using FinTrackCore.Application.Features.Reports;
using FinTrackCore.Application.Features.TransactionTypes;
using FinTrackCore.Application.Features.Transactions;
using FinTrackCore.Application.Features.UserInfos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FinTrackCore.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton(Options.Create(new MessageSettings()));
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<GoogleAuthSettings>(configuration.GetSection(GoogleAuthSettings.SectionName));
        services.Configure<SmtpSettings>(configuration.GetSection(SmtpSettings.SectionName));

        services.AddScoped<IUserInfoService, UserInfoService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAccountTypeService, AccountTypeService>();
        services.AddScoped<ICoaService, CoaService>();
        services.AddScoped<IDefaultCoaSeedService, DefaultCoaSeedService>();
        services.AddScoped<IFinancialYearService, FinancialYearService>();
        services.AddScoped<IDefaultFinancialYearSeedService, DefaultFinancialYearSeedService>();
        services.AddScoped<ITransactionTypeService, TransactionTypeService>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<IReportService, ReportService>();

        return services;
    }
}
