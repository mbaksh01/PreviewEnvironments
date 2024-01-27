using PreviewEnvironments.Application.Services.Abstractions;

namespace PreviewEnvironments.Application.Services.Abstractions;

internal interface IGitProviderFactory
{
    IGitProvider CreateProvider(GitProvider provider);
}