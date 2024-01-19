using Microsoft.Extensions.Logging;
using PreviewEnvironments.Application.Models.AzureDevOps.Builds;
using PreviewEnvironments.Application.Models.AzureDevOps.PullRequests;

namespace PreviewEnvironments.Application.Services;

internal sealed partial class PreviewEnvironmentManager
{
    private static partial class Log
    {
        [LoggerMessage(1, LogLevel.Warning, "The application configuration was not deemed to be valid. Some parts of the application not may not work as expected.", EventName = nameof(InvalidApplicationConfiguration))]
        public static partial void InvalidApplicationConfiguration(ILogger logger);
        
        [LoggerMessage(2, LogLevel.Error, "There were no available ports to start this container. Consider increasing the number of allowed ports for build definition {BuildDefinitionId}.", EventName = nameof(NoAvailablePorts))]
        public static partial void NoAvailablePorts(ILogger logger, int buildDefinitionId);
        
        [LoggerMessage(3, LogLevel.Debug, "Could not find a container linked to the pull request {PullRequestId}.", EventName = nameof(NoContainerLinkedToPr))]
        public static partial void NoContainerLinkedToPr(ILogger logger, int pullRequestId);
        
        [LoggerMessage(4, LogLevel.Debug, "Found a container linked to pull request {PullRequestId}.", EventName = nameof(ContainerLinkedToPr))]
        public static partial void ContainerLinkedToPr(ILogger logger, int pullRequestId);

        [LoggerMessage(5, LogLevel.Error, "An error occurred whilst processing a build complete message.", EventName = nameof(ErrorProcessingBuildCompleteMessage))]
        public static partial void ErrorProcessingBuildCompleteMessage(ILogger logger, Exception exception);

        [LoggerMessage(6, LogLevel.Information, "Successfully closed preview environment linked to pull request {PullRequestId}.", EventName = nameof(PreviewEnvironmentClosed))]
        public static partial void PreviewEnvironmentClosed(ILogger logger, int pullRequestId);
        
        [LoggerMessage(7, LogLevel.Information, "Failed to close preview environment linked to pull request {PullRequestId}.", EventName = nameof(ErrorClosingPreviewEnvironment))]
        public static partial void ErrorClosingPreviewEnvironment(ILogger logger, int pullRequestId);

        [LoggerMessage(8, LogLevel.Debug, "Attempting to find and stop expired containers.", EventName = nameof(FindingAndStoppingContainers))]
        public static partial void FindingAndStoppingContainers(ILogger logger);
        
        [LoggerMessage(9, LogLevel.Debug, "Found {ContainerCount} containers to expire.", EventName = nameof(FoundContainersToExpire))]
        public static partial void FoundContainersToExpire(ILogger logger, int containerCount);
        
        [LoggerMessage(10, LogLevel.Debug, "The build source branch '{BranchName}' was not for a pull request. Expected the branch name to start with 'refs/pull'.", EventName = nameof(InvalidSourceBranch))]
        public static partial void InvalidSourceBranch(ILogger logger, string branchName);
        
        [LoggerMessage(11, LogLevel.Debug, "The build status '{BuildStatus}' is not supported. Expected the build status to be Succeeded.", EventName = nameof(InvalidBuildStatus))]
        public static partial void InvalidBuildStatus(ILogger logger, BuildStatus buildStatus);
        
        [LoggerMessage(12, LogLevel.Debug, "The build definition {BuildDefinitionId} was not found in the list of supported build definitions.", EventName = nameof(BuildDefinitionNotFound))]
        public static partial void BuildDefinitionNotFound(ILogger logger, int buildDefinitionId);
        
        [LoggerMessage(13, LogLevel.Debug, "The pull request state '{PullRequestState}' is not supported. Expected Completed or Abandoned.", EventName = nameof(InvalidPullRequestState))]
        public static partial void InvalidPullRequestState(ILogger logger, PullRequestState pullRequestState);
    }
}