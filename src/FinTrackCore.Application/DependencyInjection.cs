using Microsoft.Extensions.DependencyInjection;

namespace FinTrackCore.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services;
    }
}
