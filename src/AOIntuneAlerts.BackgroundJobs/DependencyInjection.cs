using AOIntuneAlerts.BackgroundJobs.Jobs;
using Microsoft.Extensions.DependencyInjection;

namespace AOIntuneAlerts.BackgroundJobs;

public static class DependencyInjection
{
    public static IServiceCollection AddBackgroundJobs(this IServiceCollection services)
    {
        services.AddScoped<DeviceSyncJob>();
        services.AddScoped<ComplianceEvaluationJob>();
        services.AddScoped<AlertProcessingJob>();

        return services;
    }
}
