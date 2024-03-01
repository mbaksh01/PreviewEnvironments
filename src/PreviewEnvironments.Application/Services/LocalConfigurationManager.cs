using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PreviewEnvironments.Application.Helpers;
using PreviewEnvironments.Application.Models;
using PreviewEnvironments.Application.Services.Abstractions;

namespace PreviewEnvironments.Application.Services;

internal sealed partial class LocalConfigurationManager : IConfigurationManager
{
    private readonly ILogger<LocalConfigurationManager> _logger;
    private readonly IValidator<PreviewEnvironmentConfiguration> _validator;
    private readonly string _configurationFolder;
    
    private static readonly JsonSerializerOptions DefaultSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };
    
    private Dictionary<string, PreviewEnvironmentConfigurationWithPath> _configurations = [];

    public LocalConfigurationManager(
        ILogger<LocalConfigurationManager> logger,
        IOptions<ApplicationConfiguration> options,
        IValidator<PreviewEnvironmentConfiguration> validator)
    {
        _logger = logger;
        _validator = validator;
        _configurationFolder = options.Value.ConfigurationFolder;
    }

    public async Task LoadConfigurationsAsync(CancellationToken cancellationToken = default)
    {
        _configurations = [];

        string configurationPath = Path.Combine(
            Environment.CurrentDirectory,
            _configurationFolder);

        foreach (string path in Directory.GetFiles(configurationPath))
        {
            await LoadConfiguration(path, cancellationToken);
        }
    }

    public void ValidateConfigurations()
    {
        foreach (KeyValuePair<string, PreviewEnvironmentConfigurationWithPath> kvp in _configurations)
        {
            (PreviewEnvironmentConfiguration configuration, string path) =
                kvp.Value;
            
            ValidationResult result = _validator.Validate(configuration);

            if (result.IsValid)
            {
                continue;
            }

            Log.InvalidConfigurationFileValues(_logger, path);
            
            DisplayErrors(result.Errors);

            _configurations.Remove(kvp.Key);
            
            Log.InvalidConfigurationFileNoLongerTracked(_logger, path);
        }
    }

    public PreviewEnvironmentConfiguration? GetConfigurationById(
        string id)
    {
        _ = _configurations.TryGetValue(
            buildCompleteInternalBuildId,
            out PreviewEnvironmentConfigurationWithPath? value);

        return value?.Configuration;
    }

    private async Task LoadConfiguration(string path, CancellationToken cancellationToken)
    {
        if (File.Exists(path) is false)
        {
            Log.InvalidConfigurationFilePath(_logger, path);
            return;
        }
        
        using StreamReader reader = new(stream: File.Open(path, FileMode.Open));

        string content = await reader.ReadToEndAsync(cancellationToken);

        PreviewEnvironmentConfiguration? configuration = JsonSerializer
            .Deserialize<PreviewEnvironmentConfiguration>(content, DefaultSerializerOptions);

        if (configuration is null)
        {
            Log.InvalidConfigurationFileFormat(_logger, path);
            return;
        }

        string? internalBuildId = configuration.BuildServer switch
        {
            Constants.BuildServers.AzurePipelines =>
                GetAzurePipelinesId(configuration),

            _ => null,
        };

        if (string.IsNullOrWhiteSpace(internalBuildId))
        {
            Log.InvalidBuildServerName(_logger, configuration.BuildServer, path);
            return;
        }
        
        _configurations.Add(
            internalBuildId,
            new PreviewEnvironmentConfigurationWithPath(configuration, path));
    }

    private string? GetAzurePipelinesId(PreviewEnvironmentConfiguration configuration)
    {
        if (configuration.AzurePipelines is null)
        {
            Log.MissingAzurePipelinesConfiguration(_logger);
            return null;
        }

        return IdHelper.GetAzurePipelinesId(configuration.AzurePipelines);
    }
    
    private void DisplayErrors(List<ValidationFailure> resultErrors)
    {
        foreach (ValidationFailure error in resultErrors)
        {
            Log.ValidationError(
                _logger,
                error.PropertyName,
                error.ErrorMessage);
        }
    }
}

internal record PreviewEnvironmentConfigurationWithPath(
    PreviewEnvironmentConfiguration Configuration,
    string Path);