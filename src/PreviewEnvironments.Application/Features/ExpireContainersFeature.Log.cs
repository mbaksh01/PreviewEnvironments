using Microsoft.Extensions.Logging;

namespace PreviewEnvironments.Application.Features;

internal sealed partial class ExpireContainersFeature
{
    public static partial class Log
    {
        [LoggerMessage(8, LogLevel.Debug, "Attempting to find and stop expired containers.", EventName = nameof(FindingAndStoppingContainers))]
        public static partial void FindingAndStoppingContainers(ILogger logger);
        
        [LoggerMessage(9, LogLevel.Debug, "Found {ContainerCount} containers to expire.", EventName = nameof(FoundContainersToExpire))]
        public static partial void FoundContainersToExpire(ILogger logger, int containerCount);
    }
}