using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PreviewEnvironments.Application.Features;
using PreviewEnvironments.Application.Features.Abstractions;
using PreviewEnvironments.Application.Models;
using PreviewEnvironments.Application.Services;
using PreviewEnvironments.Application.Services.Abstractions;
using PreviewEnvironments.Application.Validators;
using IConfigurationManager = PreviewEnvironments.Application.Services.Abstractions.IConfigurationManager;

namespace PreviewEnvironments.Application.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the services required by the application.
    /// </summary>
    /// <param name="services">Current service collection.</param>
    /// <param name="configuration">Current configuration.</param>
    /// <returns>The <paramref name="services"/>.</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services,
        IConfiguration configuration)
    {
        _ = services
            .AddSingleton<IContainerTracker, ContainerTracker>()
            .AddSingleton<IPreviewEnvironmentManager, PreviewEnvironmentManager>()
            .AddSingleton<IConfigurationManager, LocalConfigurationManager>()
            .AddSingleton<HttpClient>()
            .AddTransient<IValidator<ApplicationConfiguration>, ApplicationConfigurationValidator>()
            .AddTransient<IValidator<PreviewEnvironmentConfiguration>, PreviewEnvironmentConfigurationValidator>()
            .AddTransient<IGitProviderFactory, GitProviderFactory>()
            .AddKeyedTransient<IGitProvider, AzureReposGitProvider>(Constants.GitProviders.AzureRepos)
            .AddTransient<IDockerService, DockerService>()
            .AddTransient<ICommandHandler, CommandHandler>()
            .AddTransient<IBuildCompleteFeature, BuildCompleteFeature>();
        
        _ = services.Configure<ApplicationConfiguration>(options =>
        {
            configuration
                .GetSection(Constants.AppSettings.Sections.Configuration)
                .Bind(options);
        });

        return services;
    }
}