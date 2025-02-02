﻿using Microsoft.Extensions.Logging;
using PreviewEnvironments.Application.Models.AzureDevOps.PullRequests;

namespace PreviewEnvironments.Application.Features;

internal sealed partial class PullRequestUpdatedFeature
{
    public static partial class Log
    {
        [LoggerMessage(3, LogLevel.Debug, "Could not find a container linked to the pull request {PullRequestId}.", EventName = nameof(NoContainerLinkedToPr))]
        public static partial void NoContainerLinkedToPr(ILogger logger, int pullRequestId);

        [LoggerMessage(6, LogLevel.Information, "Successfully closed preview environment linked to pull request {PullRequestId}.", EventName = nameof(PreviewEnvironmentClosed))]
        public static partial void PreviewEnvironmentClosed(ILogger logger, int pullRequestId);
        
        [LoggerMessage(7, LogLevel.Information, "Failed to close preview environment linked to pull request {PullRequestId}.", EventName = nameof(ErrorClosingPreviewEnvironment))]
        public static partial void ErrorClosingPreviewEnvironment(ILogger logger, int pullRequestId);
        
        [LoggerMessage(13, LogLevel.Debug, "The pull request state '{PullRequestState}' is not supported. Expected Completed or Abandoned.", EventName = nameof(PullRequestUpdatedInvalidPullRequestState))]
        public static partial void PullRequestUpdatedInvalidPullRequestState(ILogger logger, PullRequestState pullRequestState);
    }
}