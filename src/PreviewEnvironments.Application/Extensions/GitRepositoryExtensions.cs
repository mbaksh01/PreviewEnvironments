using PreviewEnvironments.Application.Services.Abstractions;

namespace PreviewEnvironments.Application.Extensions;

internal static class GitRepositoryExtensions
{
    private const string ExpiredContainerMessage = """
        Preview environment has been stopped to save resources.
        To restart the container, follow the link posted to this pull request, re-queue the build or use the command `/pe restart`.
        If your containers stop too early consider increasing the containerTimeoutSeconds for this project.
        """;
    
    public static Task PostContainerNotFoundMessageAsync(
        this IGitProvider gitProvider,
        string internalConfigId,
        int pullRequestId,
        CancellationToken cancellationToken = default)
    {
        return gitProvider.PostPullRequestMessageAsync(
            internalConfigId,
            pullRequestId,
            "Could not find a container linked to this pull request. Try re-queueing the build to register your pull request with this service.",
            cancellationToken);
    }

    /// <summary>
    /// Posts the preview available message to Azure DevOps.
    /// </summary>
    /// <param name="gitProvider"></param>
    /// <param name="internalConfigId">
    /// Id used to get the correct configuration file.
    /// </param>
    /// <param name="pullRequestId">Id of pull request.</param>
    /// <param name="containerAddress">
    /// Address where container can be accessed.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancellation token used to stop this task.
    /// </param>

    public static Task PostPreviewAvailableMessageAsync(
        this IGitProvider gitProvider,
        string internalConfigId,
        int pullRequestId,
        Uri containerAddress,
        CancellationToken cancellationToken = default)
    {
        return gitProvider.PostPullRequestMessageAsync(
            internalConfigId,
            pullRequestId,
            $"Preview environment available at [{containerAddress}]({containerAddress}).",
            cancellationToken);
    }

    /// <summary>
    /// Posts a message to the pull request stating a container has been stopped.
    /// </summary>
    /// <param name="gitProvider"></param>
    /// <param name="internalConfigId">
    /// Id used to get the correct configuration file.
    /// </param>
    /// <param name="pullRequestId">Pull request number to post to.</param>
    /// <param name="cancellationToken">
    /// Cancellation token used to stop this task.
    /// </param>
    public static Task PostExpiredContainerMessageAsync(
        this IGitProvider gitProvider,
        string internalConfigId,
        int pullRequestId,
        CancellationToken cancellationToken = default)
    {
        return gitProvider.PostPullRequestMessageAsync(
            internalConfigId,
            pullRequestId,
            ExpiredContainerMessage,
            cancellationToken);
    }
}