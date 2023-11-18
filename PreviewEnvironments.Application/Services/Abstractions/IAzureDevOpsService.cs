using PreviewEnvironments.Application.Models.AzureDevOps.Builds;
using PreviewEnvironments.Application.Models.AzureDevOps.PullRequests;

namespace PreviewEnvironments.Application.Services.Abstractions;
public interface IAzureDevOpsService
{
    Task BuildCompleteAsync(BuildComplete buildComplete, CancellationToken cancellationToken = default);

    Task PullRequestUpdatedAsync(PullRequestUpdated pullRequestUpdated, CancellationToken cancellationToken = default);
}