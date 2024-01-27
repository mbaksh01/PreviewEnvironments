using Microsoft.Extensions.Logging;

namespace PreviewEnvironments.Application.Services;

internal partial class LocalConfigurationManager
{
    public static partial class Log
    {
        [LoggerMessage(1, LogLevel.Warning, "The configuration file found at path: '{ConfigurationFilePath}' was not formatted correctly.", EventName = nameof(InvalidConfigurationFileFormat))]
        public static partial void InvalidConfigurationFileFormat(ILogger logger, string configurationFilePath);
        
        [LoggerMessage(2, LogLevel.Warning, "The build server configuration was not valid for configuration file: '{ConfigurationFilePath}'.", EventName = nameof(InvalidBuildServerConfiguration))]
        public static partial void InvalidBuildServerConfiguration(ILogger logger, string configurationFilePath);
    }
}