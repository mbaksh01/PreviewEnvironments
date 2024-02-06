using Microsoft.Extensions.Logging;

namespace PreviewEnvironments.Application.Services;

internal partial class LocalConfigurationManager
{
    public static partial class Log
    {
        [LoggerMessage(1, LogLevel.Warning, "The configuration file found at path: '{ConfigurationFilePath}' was not formatted correctly.", EventName = nameof(InvalidConfigurationFileFormat))]
        public static partial void InvalidConfigurationFileFormat(ILogger logger, string configurationFilePath);
        
        [LoggerMessage(2, LogLevel.Warning, "The build server name '{BuildServerName}' was found in '{ConfigurationFilePath}' but is not valid.", EventName = nameof(InvalidBuildServerName))]
        public static partial void InvalidBuildServerName(ILogger logger, string buildServerName, string configurationFilePath);
    }
}