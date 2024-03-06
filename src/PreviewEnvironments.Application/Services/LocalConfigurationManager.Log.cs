using Microsoft.Extensions.Logging;

namespace PreviewEnvironments.Application.Services;

internal partial class LocalConfigurationManager
{
    public static partial class Log
    {
        [LoggerMessage(1, LogLevel.Warning, "The configuration file found at path: '{ConfigurationFilePath}' was not formatted correctly.", EventName = nameof(InvalidConfigurationFileFormat))]
        public static partial void InvalidConfigurationFileFormat(ILogger logger, string configurationFilePath);
        
        [LoggerMessage(2, LogLevel.Warning, "Unable to determine a suitable id for configuration file '{ConfigurationFilePath}'.", EventName = nameof(UnableToDetermineASuitableId))]
        public static partial void UnableToDetermineASuitableId(ILogger logger, string configurationFilePath);
        
        [LoggerMessage(3, LogLevel.Warning, "The configuration file path '{ConfigurationFilePath}' was not found.", EventName = nameof(InvalidConfigurationFilePath))]
        public static partial void InvalidConfigurationFilePath(ILogger logger, string configurationFilePath);

        [LoggerMessage(4, LogLevel.Warning, "The stated build provider was AzurePipelines but no matching configuration was found. Ensure that your configuration file contains the \"azurePipelines\" section.", EventName = nameof(MissingAzurePipelinesConfiguration))]
        public static partial void MissingAzurePipelinesConfiguration(ILogger logger);
        
        [LoggerMessage(5, LogLevel.Warning, "Validation for configuration file path '{ConfigurationFilePath}' failed.", EventName = nameof(InvalidConfigurationFileValues))]
        public static partial void InvalidConfigurationFileValues(ILogger logger, string configurationFilePath);

        [LoggerMessage(6, LogLevel.Warning, "=> PropertyName: {PropertyName}. Error Message: {ErrorMessage}", EventName = nameof(ValidationError))]
        public static partial void ValidationError(ILogger logger, string propertyName, string errorMessage);

        [LoggerMessage(7, LogLevel.Warning, "Removed tracking of configuration file '{ConfigurationFilePath}'.", EventName = nameof(InvalidConfigurationFileNoLongerTracked))]
        public static partial void InvalidConfigurationFileNoLongerTracked(ILogger logger, string configurationFilePath);

        [LoggerMessage(8, LogLevel.Warning, "The stated git provider was AzureRepos but no matching configuration was found. Ensure that your configuration files contains the \"azureRepos\" section.", EventName = nameof(MissingAzureReposConfiguration))]
        public static partial void MissingAzureReposConfiguration(ILogger logger);

        [LoggerMessage(9, LogLevel.Error, "The configuration file found at path: '{ConfigurationFilePath}' does not contain valid JSON.", EventName = nameof(InvalidConfigurationFileJson))]
        public static partial void InvalidConfigurationFileJson(ILogger logger, Exception exception, string configurationFilePath);
    }
}