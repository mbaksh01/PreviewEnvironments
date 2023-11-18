using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PreviewEnvironments.Application.Services;
using PreviewEnvironments.Application.Services.Abstractions;

namespace PreviewEnvironments.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services
            .AddScoped<IAzureDevOpsService>(sp => new AzureDevOpsService(
                    sp.GetRequiredService<ILogger<AzureDevOpsService>>(),
                    sp.GetRequiredService<IDockerService>(),
                    sp.GetRequiredService<HttpClient>(),
                    sp.GetAppConfiguration()
                )
            )
            .AddSingleton<IDockerService, DockerService>()
            .AddScoped<HttpClient>()
            .AddSingleton<ApplicationLifetimeService>();
    }
}
