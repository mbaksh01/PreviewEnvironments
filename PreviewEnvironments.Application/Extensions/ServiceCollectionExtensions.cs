﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PreviewEnvironments.Application.Models;
using PreviewEnvironments.Application.Services;
using PreviewEnvironments.Application.Services.Abstractions;

namespace PreviewEnvironments.Application.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the services required by the application.
    /// </summary>
    /// <param name="services">Current service collection.</param>
    /// <param name="configuration">Current configuration.</param>
    /// <returns>The <paramref name="services"/>.</returns>
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