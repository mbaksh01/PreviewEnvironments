using PreviewEnvironments.Application.Models.AzureDevOps.Builds;
using PreviewEnvironments.Application.Models.AzureDevOps.PullRequests;

namespace PreviewEnvironments.Application.Services.Abstractions;

public interface IPreviewEnvironmentManager : IAsyncDisposable
{
    /// <summary>
    /// Initialises this service and its dependencies.
    /// </summary>
    /// <returns></returns>
    Task InitialiseAsync(CancellationToken cancellationToken = default);
}