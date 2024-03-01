using Microsoft.Extensions.Logging;
using PreviewEnvironments.Application.Models.AzureDevOps.Builds;
using PreviewEnvironments.Application.Models.AzureDevOps.PullRequests;

namespace PreviewEnvironments.Application.Services;

internal sealed partial class PreviewEnvironmentManager
{
    private static partial class Log
    {
        [LoggerMessage(8, LogLevel.Debug, "Attempting to find and stop expired containers.", EventName = nameof(FindingAndStoppingContainers))]
        public static partial void FindingAndStoppingContainers(ILogger logger);
        
        [LoggerMessage(9, LogLevel.Debug, "Found {ContainerCount} containers to expire.", EventName = nameof(FoundContainersToExpire))]
        public static partial void FoundContainersToExpire(ILogger logger, int containerCount);
    }
}