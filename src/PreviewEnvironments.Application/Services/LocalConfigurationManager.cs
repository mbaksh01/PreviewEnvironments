using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PreviewEnvironments.Application.Helpers;
using PreviewEnvironments.Application.Models;
using PreviewEnvironments.Application.Services.Abstractions;

namespace PreviewEnvironments.Application.Services;

internal sealed partial class LocalConfigurationManager : IConfigurationManager
{
    private readonly ILogger<LocalConfigurationManager> _logger;
    private readonly string _configurationFolder;
    
    private static readonly JsonSerializerOptions DefaultSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
    
    private Dictionary<string, PreviewEnvironmentConfiguration> _configurations = [];

    public LocalConfigurationManager(
        ILogger<LocalConfigurationManager> logger,
        IOptions<ApplicationConfiguration> options)
    {
        _logger = logger;
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
        throw new NotImplementedException();
    }

    public PreviewEnvironmentConfiguration? GetConfigurationByBuildId(
        string buildCompleteInternalBuildId)
    {
        _ = _configurations.TryGetValue(
            buildCompleteInternalBuildId,
            out PreviewEnvironmentConfiguration? configuration);

        return configuration;
    }

    private async Task LoadConfiguration(string path, CancellationToken cancellationToken)
    {
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
                IdHelper.GetAzurePipelinesId(configuration.AzurePipelines!),

            _ => null,
        };

        if (string.IsNullOrWhiteSpace(internalBuildId))
        {
            Log.InvalidBuildServerName(_logger, configuration.BuildServer, path);
            return;
        }
        
        _configurations.Add(internalBuildId, configuration);
    }
}