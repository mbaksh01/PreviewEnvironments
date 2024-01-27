using Microsoft.Extensions.DependencyInjection;
using PreviewEnvironments.Application.Services.Abstractions;

namespace PreviewEnvironments.Application.Services;

internal sealed class GitProviderFactory : IGitProviderFactory
{
    private readonly IServiceProvider _serviceProvider;

    public GitProviderFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public IGitProvider CreateProvider(GitProvider provider)
    {
        return provider switch
        {
            GitProvider.AzureRepos => _serviceProvider.GetRequiredKeyedService<IGitProvider>(Constants.GitProviders.AzureRepos),
            _ => throw new NotSupportedException($"The git provider '{provider}' is not supported.")
        };
    }
}

public enum GitProvider
{
    AzureRepos,
}