using Microsoft.Extensions.Logging;

namespace PreviewEnvironments.Application.Features;

internal partial class RedirectFeature
{
    public static partial class Log
    {
        [LoggerMessage(1, LogLevel.Warning, "Could not find container with id '{ContainerId}'.", EventName = nameof(ContainerNotFound))]
        public static partial void ContainerNotFound(ILogger logger, string containerId);
        
        [LoggerMessage(2, LogLevel.Debug, "Container {ContainerId} is expired. Attempting to start container.")]
        public static partial void StartingContainer(ILogger logger, string containerId);
    }
}