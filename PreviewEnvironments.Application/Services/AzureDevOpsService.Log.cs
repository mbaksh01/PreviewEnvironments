using Microsoft.Extensions.Logging;
using PreviewEnvironments.Application.Models.AzureDevOps.PullRequests;

namespace PreviewEnvironments.Application.Services;

internal sealed partial class AzureDevOpsService
{
    private static partial class Log
    {
        [LoggerMessage(1, LogLevel.Information, "Successfully posted status as '{PullRequestStatus}'.", EventName = nameof(PostedStatusSuccessfully))]
        public static partial void PostedStatusSuccessfully(ILogger logger, PullRequestStatusState pullRequestStatus);

        [LoggerMessage(2, LogLevel.Error, "Failed to post status.", EventName = nameof(PostedStatusFailed))]
        public static partial void PostedStatusFailed(ILogger logger, Exception ex);

        [LoggerMessage(3, LogLevel.Error, "Azure DevOps Api Response: {ApiResponse}", EventName = nameof(AzureDevOpsApiResponseError))]
        public static partial void AzureDevOpsApiResponseError(ILogger logger, string apiResponse);
    }
}