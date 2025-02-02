﻿using Microsoft.Extensions.Logging;
using PreviewEnvironments.Application.Models.AzureDevOps.PullRequests;

namespace PreviewEnvironments.Application.Services;

internal sealed partial class AzureReposGitProvider
{
    private static partial class Log
    {
        [LoggerMessage(1, LogLevel.Information, "Successfully posted status as '{PullRequestStatus}'.", EventName = nameof(PostedStatusSuccessfully))]
        public static partial void PostedStatusSuccessfully(ILogger logger, PullRequestStatusState pullRequestStatus);

        [LoggerMessage(2, LogLevel.Error, "Failed to post status.", EventName = nameof(PostedStatusFailed))]
        public static partial void PostedStatusFailed(ILogger logger, Exception ex);

        [LoggerMessage(3, LogLevel.Error, "Azure DevOps Api Response: {ApiResponse}", EventName = nameof(AzureDevOpsApiResponseError))]
        public static partial void AzureDevOpsApiResponseError(ILogger logger, string apiResponse);
        
        [LoggerMessage(4, LogLevel.Information, "Successfully posted expired container thread for pull request {PullRequestId}.", EventName = nameof(PostedExpiredContainerMessage))]
        public static partial void PostedExpiredContainerMessage(ILogger logger, int pullRequestId);
        
        [LoggerMessage(5, LogLevel.Information, "Successfully posted message for pull request {PullRequestId}.", EventName = nameof(PostedMessage))]
        public static partial void PostedMessage(ILogger logger, int pullRequestId);
        
        [LoggerMessage(6, LogLevel.Error, "Failed to post the message for pull request {PullRequestId}.", EventName = nameof(PostMessageFailed))]
        public static partial void PostMessageFailed(ILogger logger, Exception exception, int pullRequestId);

        [LoggerMessage(7, LogLevel.Error, "Failed to get the pull request for pull request {PullRequestId}.", EventName = nameof(GetPullRequestByIdFailed))]
        public static partial void GetPullRequestByIdFailed(ILogger logger, Exception exception, int pullRequestId);
        
        [LoggerMessage(8, LogLevel.Debug, "Successfully got the pull request for pull request {PullRequestId}.", EventName = nameof(GetPullRequestByIdSucceeded))]
        public static partial void GetPullRequestByIdSucceeded(ILogger logger, int pullRequestId);

        [LoggerMessage(9, LogLevel.Warning, "Failed to determine the configuration file linked to internal build id: '{InternalBuildId}'.", EventName = nameof(ConfigurationNotFound))]
        public static partial void ConfigurationNotFound(ILogger logger, string internalBuildId);

        [LoggerMessage(10, LogLevel.Debug, "Successfully got the current iteration id ({IterationId}) for pull request {PullRequestId}.", EventName = nameof(GetPullRequestIterationSucceeded))]
        public static partial void GetPullRequestIterationSucceeded(ILogger logger, int iterationId, int pullRequestId);

        [LoggerMessage(11, LogLevel.Debug, "Failed to determine the iteration id for pull request {PullRequestId}.", EventName = nameof(GetPullRequestIterationFailed))]
        public static partial void GetPullRequestIterationFailed(ILogger logger, Exception exception, int pullRequestId);
    }
}