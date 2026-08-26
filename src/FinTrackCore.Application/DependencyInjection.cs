using FinTrackCore.Application.Common.Configuration;
using FinTrackCore.Application.Features.Auth;
using FinTrackCore.Application.Features.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FinTrackCore.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<MessageSettings>(configuration.GetSection(MessageSettings.SectionName));
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.AddScoped<IUserInfoService, UserInfoService>();
        services.AddScoped<IAuthService, AuthService>();
        return services;
    }
}
