using System.Text;
using System.Text.Json;
using FinTrackCore.Application.Common.Configuration;
using FinTrackCore.Application.Common.Models;
using FinTrackCore.Application.Constants;
using FinTrackCore.Application.Interfaces;
using FinTrackCore.Domain.Repositories;
using FinTrackCore.Infrastructure.Persistence;
using FinTrackCore.Infrastructure.Persistence.Repositories;
using FinTrackCore.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FinTrackCore.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
            ?? throw new InvalidOperationException("Jwt settings are missing.");

        if (string.IsNullOrWhiteSpace(jwtSettings.Key) || jwtSettings.Key.Length < AuthConstants.JwtMinKeyLength)
        {
            throw new InvalidOperationException(
                $"Jwt:Key must be configured and at least {AuthConstants.JwtMinKeyLength} characters.");
        }

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";

                        var messages = context.HttpContext.RequestServices
                            .GetRequiredService<IOptions<MessageSettings>>()
                            .Value;

                        var body = new ApiResponse<object?>
                        {
                            Success = false,
                            StatusCode = StatusCodes.Status401Unauthorized,
                            Message = messages.Unauthorized,
                            Data = null,
                            Meta = null
                        };

                        await context.Response.WriteAsync(JsonSerializer.Serialize(body, new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        }));
                    },
                    OnForbidden = async context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        context.Response.ContentType = "application/json";

                        var messages = context.HttpContext.RequestServices
                            .GetRequiredService<IOptions<MessageSettings>>()
                            .Value;

                        var body = new ApiResponse<object?>
                        {
                            Success = false,
                            StatusCode = StatusCodes.Status403Forbidden,
                            Message = messages.Forbidden,
                            Data = null,
                            Meta = null
                        };

                        await context.Response.WriteAsync(JsonSerializer.Serialize(body, new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        }));
                    }
                };
            });

        services.AddAuthorization();

        services.AddScoped<IUserInfoRepository, UserInfoRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IAccountTypeRepository, AccountTypeRepository>();
        services.AddScoped<ICoaRepository, CoaRepository>();
        services.AddScoped<IFinancialYearRepository, FinancialYearRepository>();
        services.AddScoped<ITransactionTypeRepository, TransactionTypeRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IReportRepository, ReportRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IGoogleAuthService, GoogleAuthService>();

        return services;
    }
}
