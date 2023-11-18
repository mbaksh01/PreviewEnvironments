using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PreviewEnvironments.Application.Models;

namespace PreviewEnvironments.Application.Extensions;

internal static class ServiceProviderExtensions
{
    public static ApplicationConfiguration GetAppConfiguration(this IServiceProvider serviceProvider)
    {
        ApplicationConfiguration configuration = new();

        serviceProvider
            .GetRequiredService<IConfiguration>()
            .GetSection(Constants.AppSettings.Sections.Configuration)
            .Bind(configuration);

        string token = Environment.GetEnvironmentVariable(Constants.EnvVariables.AzAccessToken)
            ?? throw new Exception(
                $"{Constants.EnvVariables.AzAccessToken} was not present" +
                " in the environmental variables. This token is required to" +
                " interact with Azure DevOps APIs."
            );

        configuration.AzAccessToken = token;
        // TODO: Remove
        configuration.AzAccessToken = "v7x4lwoidwji65sgz2krquvltinn4cqj7bvrwgtxqpatqaarn23q";

        return configuration;
    }
}
