using PreviewEnvironments.Application.Models.AzureDevOps.PullRequests;

namespace PreviewEnvironments.Application.Features.Abstractions;

public interface IPullRequestUpdatedFeature
{
    /// <summary>
    /// Takes information about a updated pull request and performs the required
    /// action on its associated preview environment.
    /// </summary>
    /// <param name="pullRequestUpdated">
    /// Information about the updated pull request.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancellation token used to stop this task.
    /// </param>
    /// <returns></returns>
    ValueTask PullRequestUpdatedAsync(
        PullRequestUpdated pullRequestUpdated,
        CancellationToken cancellationToken = default);
}