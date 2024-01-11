using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PreviewEnvironments.Application.Models;
using PreviewEnvironments.Application.Services;
using PreviewEnvironments.Application.Services.Abstractions;

namespace PreviewEnvironments.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        _ = services
            .AddSingleton<IAzureDevOpsService, AzureDevOpsService>()
            .AddSingleton<IDockerService, DockerService>()
            .AddSingleton<HttpClient>()
            .AddSingleton<ApplicationLifetimeService>();
        
        _ = services.Configure<ApplicationConfiguration>(options =>
        {
            configuration.GetSection(Constants.AppSettings.Sections.Configuration).Bind(options);
        });

        return services;
    }
}