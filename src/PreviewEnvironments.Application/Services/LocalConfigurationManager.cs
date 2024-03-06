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
    
    private List<PreviewEnvironmentConfigurationExtended> _configurations = [];

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
        List<PreviewEnvironmentConfigurationExtended> configurationsToRemove = [];
        
        foreach (PreviewEnvironmentConfigurationExtended configurationExtended in _configurations)
        {
            (_, PreviewEnvironmentConfiguration configuration, string path) =
                configurationExtended;
            
            ValidationResult result = _validator.Validate(configuration);

            if (result.IsValid)
            {
                continue;
            }

            Log.InvalidConfigurationFileValues(_logger, path);
            
            DisplayErrors(result.Errors);

            configurationsToRemove.Add(configurationExtended);
            
            Log.InvalidConfigurationFileNoLongerTracked(_logger, path);
        }

        foreach (PreviewEnvironmentConfigurationExtended configuration in configurationsToRemove)
        {
            _configurations.Remove(configuration);
        }
    }

    public PreviewEnvironmentConfiguration? GetConfigurationById(
        string id)
    {
        return _configurations
            .SingleOrDefault(c => c.Ids.Contains(id))?
            .Configuration;
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

        PreviewEnvironmentConfiguration? configuration;
        
        try
        {
            configuration = JsonSerializer
                .Deserialize<PreviewEnvironmentConfiguration>(content, DefaultSerializerOptions);
        }
        catch (JsonException ex)
        {
            Log.InvalidConfigurationFileJson(_logger, ex, path);
            return;
        }

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

        string? internalRepoId = configuration.GitProvider switch
        {
            Constants.GitProviders.AzureRepos => GetAzureReposId(configuration),
            _ => null,
        };

        string?[] ids = [internalBuildId, internalRepoId];

        if (ids.Any(id => !string.IsNullOrWhiteSpace(id)) == false)
        {
            Log.UnableToDetermineASuitableId(_logger, path);
            return;
        }
        
        _configurations.Add(
            new PreviewEnvironmentConfigurationExtended(ids, configuration, path));
    }

    private string? GetAzureReposId(PreviewEnvironmentConfiguration configuration)
    {
        if (configuration.AzureRepos is null)
        {
            Log.MissingAzureReposConfiguration(_logger);
            return null;
        }

        return IdHelper.GetAzureReposId(configuration.AzureRepos);
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

internal record PreviewEnvironmentConfigurationExtended(
    string?[] Ids,
    PreviewEnvironmentConfiguration Configuration,
    string Path);
    