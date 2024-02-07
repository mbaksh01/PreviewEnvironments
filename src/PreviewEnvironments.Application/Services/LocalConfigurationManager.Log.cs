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
        
        [LoggerMessage(3, LogLevel.Warning, "The configuration file path '{ConfigurationFilePath}' was not found.", EventName = nameof(InvalidConfigurationFilePath))]
        public static partial void InvalidConfigurationFilePath(ILogger logger, string configurationFilePath);

        [LoggerMessage(4, LogLevel.Warning, "The stated build provider was AzurePipelines but no matching configuration was found. Ensure that your configuration file contains the \"azurePipelines\" section.")]
        public static partial void MissingAzurePipelinesConfiguration(ILogger logger);
    }
}