using Microsoft.Extensions.Logging;
using PreviewEnvironments.Application.Models.AzureDevOps.Builds;
using PreviewEnvironments.Application.Models.AzureDevOps.PullRequests;

namespace PreviewEnvironments.Application.Features;

internal sealed partial class BuildCompleteFeature
{
    public static partial class Log
    {
        [LoggerMessage(1, LogLevel.Error, "There were no available ports to start this container. Consider increasing the number of allowed ports for build configuration '{InternalBuildId}'.", EventName = nameof(NoAvailablePorts))]
        public static partial void NoAvailablePorts(ILogger logger, string internalBuildId);

        [LoggerMessage(2, LogLevel.Debug, "Found a container linked to pull request {PullRequestId}.", EventName = nameof(ContainerLinkedToPr))]
        public static partial void ContainerLinkedToPr(ILogger logger, int pullRequestId);

        [LoggerMessage(3, LogLevel.Error, "An error occurred whilst processing a build complete message.", EventName = nameof(ErrorProcessingBuildCompleteMessage))]
        public static partial void ErrorProcessingBuildCompleteMessage(ILogger logger, Exception exception);

        [LoggerMessage(4, LogLevel.Debug, "The build source branch '{BranchName}' was not for a pull request. Expected the branch name to start with 'refs/pull'.", EventName = nameof(InvalidSourceBranch))]
        public static partial void InvalidSourceBranch(ILogger logger, string branchName);

        [LoggerMessage(5, LogLevel.Debug, "The build status '{BuildStatus}' is not supported. Expected the build status to be Succeeded.", EventName = nameof(InvalidBuildStatus))]
        public static partial void InvalidBuildStatus(ILogger logger, BuildStatus buildStatus);

        [LoggerMessage(6, LogLevel.Debug, "The pull request state '{PullRequestState}' is not supported. Expected Active.", EventName = nameof(BuildCompleteInvalidPullRequestState))]
        public static partial void BuildCompleteInvalidPullRequestState(ILogger logger, PullRequestState pullRequestState);

        [LoggerMessage(7, LogLevel.Warning, "The pull request with id {PullRequestId} was not found. This may mean the applications configuration is invalid.", EventName = nameof(PullRequestNotFound))]
        public static partial void PullRequestNotFound(ILogger logger, int pullRequestId);

        [LoggerMessage(8, LogLevel.Debug, "The preview environment configuration with id '{InternalBuildId}' was not found.", EventName = nameof(PreviewEnvironmentConfigurationNotFound))]
        public static partial void PreviewEnvironmentConfigurationNotFound(ILogger logger, string internalBuildId);
        
        [LoggerMessage(9, LogLevel.Debug, "Could not find a container linked to the pull request {PullRequestId}.", EventName = nameof(NoContainerLinkedToPr))]
        public static partial void NoContainerLinkedToPr(ILogger logger, int pullRequestId);
    }
}