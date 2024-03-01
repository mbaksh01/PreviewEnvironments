using Microsoft.Extensions.Logging;
using PreviewEnvironments.Application.Models.AzureDevOps.Builds;
using PreviewEnvironments.Application.Models.AzureDevOps.PullRequests;

namespace PreviewEnvironments.Application.Services;

internal sealed partial class PreviewEnvironmentManager
{
    private static partial class Log
    {
        [LoggerMessage(3, LogLevel.Debug, "Could not find a container linked to the pull request {PullRequestId}.", EventName = nameof(NoContainerLinkedToPr))]
        public static partial void NoContainerLinkedToPr(ILogger logger, int pullRequestId);

        [LoggerMessage(6, LogLevel.Information, "Successfully closed preview environment linked to pull request {PullRequestId}.", EventName = nameof(PreviewEnvironmentClosed))]
        public static partial void PreviewEnvironmentClosed(ILogger logger, int pullRequestId);
        
        [LoggerMessage(7, LogLevel.Information, "Failed to close preview environment linked to pull request {PullRequestId}.", EventName = nameof(ErrorClosingPreviewEnvironment))]
        public static partial void ErrorClosingPreviewEnvironment(ILogger logger, int pullRequestId);

        [LoggerMessage(8, LogLevel.Debug, "Attempting to find and stop expired containers.", EventName = nameof(FindingAndStoppingContainers))]
        public static partial void FindingAndStoppingContainers(ILogger logger);
        
        [LoggerMessage(9, LogLevel.Debug, "Found {ContainerCount} containers to expire.", EventName = nameof(FoundContainersToExpire))]
        public static partial void FoundContainersToExpire(ILogger logger, int containerCount);
        
        [LoggerMessage(13, LogLevel.Debug, "The pull request state '{PullRequestState}' is not supported. Expected Completed or Abandoned.", EventName = nameof(PullRequestUpdatedInvalidPullRequestState))]
        public static partial void PullRequestUpdatedInvalidPullRequestState(ILogger logger, PullRequestState pullRequestState);
    }
}