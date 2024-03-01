using PreviewEnvironments.Application.Services.Abstractions;

namespace PreviewEnvironments.Application.Extensions;

internal static class GitRepositoryExtensions
{
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
}