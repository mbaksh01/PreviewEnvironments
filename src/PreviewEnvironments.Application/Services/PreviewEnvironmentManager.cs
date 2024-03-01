using System.Diagnostics;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PreviewEnvironments.Application.Extensions;
using PreviewEnvironments.Application.Models;
using PreviewEnvironments.Application.Models.AzureDevOps.Builds;
using PreviewEnvironments.Application.Models.AzureDevOps.Contracts;
using PreviewEnvironments.Application.Models.AzureDevOps.PullRequests;
using PreviewEnvironments.Application.Models.Docker;
using PreviewEnvironments.Application.Services.Abstractions;

namespace PreviewEnvironments.Application.Services;

internal sealed partial class PreviewEnvironmentManager : IPreviewEnvironmentManager
{
    private readonly ILogger<PreviewEnvironmentManager> _logger;
    private readonly IValidator<ApplicationConfiguration> _validator;
    private readonly IGitProviderFactory _gitProviderFactory;
    private readonly IDockerService _dockerService;
    private readonly IConfigurationManager _configurationManager;
    private readonly IContainerTracker _containers;
    private readonly ApplicationConfiguration _configuration;
    
    public PreviewEnvironmentManager(
        ILogger<PreviewEnvironmentManager> logger,
        IValidator<ApplicationConfiguration> validator,
        IOptions<ApplicationConfiguration> configuration,
        IGitProviderFactory gitProviderFactory,
        IDockerService dockerService,
        IConfigurationManager configurationManager,
        IContainerTracker containers)
    {
        _logger = logger;
        _validator = validator;
        _gitProviderFactory = gitProviderFactory;
        _dockerService = dockerService;
        _configurationManager = configurationManager;
        _containers = containers;
        _configuration = configuration.Value;
    }

    public async Task InitialiseAsync(CancellationToken cancellationToken = default)
    {
        await _configurationManager.LoadConfigurationsAsync(cancellationToken);
        _configurationManager.ValidateConfigurations();
        await _dockerService.InitialiseAsync(cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        ICollection<string> containerIds = _containers.GetKeys();
        
        foreach (string containerId in containerIds)
        {
            await _dockerService.StopAndRemoveContainerAsync(containerId);
        }
        
        _dockerService.Dispose();
    }
}