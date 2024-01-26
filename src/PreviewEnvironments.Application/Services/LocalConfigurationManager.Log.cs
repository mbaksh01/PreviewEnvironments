using Microsoft.Extensions.Logging;

namespace PreviewEnvironments.Application.Services;

internal partial class LocalConfigurationManager
{
    public static partial class Log
    {
        [LoggerMessage(1, LogLevel.Warning, "The configuration file found at path: '{ConfigurationFilePath}' was invalid.", EventName = nameof(InvalidConfigurationFile))]
        public static partial void InvalidConfigurationFile(ILogger logger, string configurationFilePath);
    }
}