using Microsoft.Extensions.Logging;

namespace PreviewEnvironments.Application.Services;

internal partial class CommandHandler
{
    public static partial class Log
    {
        [LoggerMessage(1, LogLevel.Warning, "Could not find container linked to pull request id: {PullRequestId}.", EventName = nameof(ContainerNotFound))]
        public static partial void ContainerNotFound(ILogger logger, int pullRequestId);
        
        [LoggerMessage(2, LogLevel.Warning,"Failed to restart container linked to pull request id: {PullRequestId}.", EventName = nameof(FailedToStartContainer))]
        public static partial void FailedToStartContainer(ILogger logger, int pullRequestId);

        [LoggerMessage(3, LogLevel.Warning,"Failed to determine configuration file linked to internal build id: '{InternalBuildId}'.", EventName = nameof(ConfigurationNotFound))]
        public static partial void ConfigurationNotFound(ILogger logger, string internalBuildId);
    }
}