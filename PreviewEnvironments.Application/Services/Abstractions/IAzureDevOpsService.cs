using PreviewEnvironments.Application.Models.AzureDevOps;
using PreviewEnvironments.Application.Models.AzureDevOps.PullRequests;

namespace PreviewEnvironments.Application.Services.Abstractions;

internal interface IAzureDevOpsService
{
    /// <summary>
    /// Posts the <see cref="PreviewAvailableMessage"/> to Azure DevOps.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="cancellationToken">
    /// Cancellation token used to stop this task.
    /// </param>
    Task PostPreviewAvailableMessageAsync(PreviewAvailableMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Posts a message to the pull request stating a container has been stopped.
    /// </summary>
    /// <param name="pullRequestNumber">Pull request number to post to.</param>
    /// <param name="cancellationToken">
    /// Cancellation token used to stop this task.
    /// </param>
    Task PostExpiredContainerMessageAsync(int pullRequestNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Posts a pull request status to Azure DevOps using the
    /// <paramref name="message"/>.
    /// </summary>
    /// <param name="message">Information about the status.</param>
    /// <param name="cancellationToken">
    /// Cancellation token used to stop this task.
    /// </param>
    Task PostPullRequestStatusAsync(PullRequestStatusMessage message, CancellationToken cancellationToken = default);
}