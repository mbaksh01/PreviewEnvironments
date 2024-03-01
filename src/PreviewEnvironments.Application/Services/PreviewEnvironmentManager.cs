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

    /// <summary>
    /// Takes information about a updated pull request and performs the required
    /// action on its associated preview environment.
    /// </summary>
    /// <param name="pullRequestUpdated">
    /// Information about the updated pull request.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancellation token used to stop this task.
    /// </param>
    /// <returns></returns>
    public async ValueTask PullRequestUpdatedAsync(
        PullRequestUpdated pullRequestUpdated,
        CancellationToken cancellationToken = default)
    {
        if (pullRequestUpdated.State is PullRequestState.Active)
        {
            Log.PullRequestUpdatedInvalidPullRequestState(_logger, pullRequestUpdated.State);
            return;
        }
        
        int pullRequestId = pullRequestUpdated.Id;

        string? containerId = _containers.SingleOrDefault(c => c.PullRequestId == pullRequestId)?.ContainerId;

        if (string.IsNullOrWhiteSpace(containerId))
        {
            Log.NoContainerLinkedToPr(_logger, pullRequestId);
            return;
        }
        
        bool response = await _dockerService.StopAndRemoveContainerAsync(
            containerId,
            cancellationToken
        );

        if (response is false)
        {
            Log.ErrorClosingPreviewEnvironment(_logger, pullRequestId);
            return;
        }

        _ = _containers.Remove(containerId);

        Log.PreviewEnvironmentClosed(_logger, pullRequestId);
    }
    
    /// <inheritdoc />
    public async Task ExpireContainersAsync(CancellationToken cancellationToken = default)
    {
        Log.FindingAndStoppingContainers(_logger);

        IEnumerable<DockerContainer> runningContainers = _containers
            .Where(c => c is { Expired: false, CanExpire: true });

        List<DockerContainer> expiredContainers = [];

        foreach (DockerContainer container in runningContainers)
        {
            Deployment? deployment = _configurationManager
                .GetConfigurationById(container.InternalBuildId)?.Deployment;

            if (deployment is null)
            {
                continue;
            }

            TimeSpan timeout =
                TimeSpan.FromSeconds(deployment.ContainerTimeoutSeconds);

            if (container.CreatedTime + timeout >= DateTimeOffset.Now)
            {
                continue;
            }

            expiredContainers.Add(container);
        }

        Log.FoundContainersToExpire(_logger, expiredContainers.Count);

        foreach (DockerContainer expiredContainer in expiredContainers)
        {
            expiredContainer.Expired = await _dockerService.StopContainerAsync(
                expiredContainer.ContainerId,
                cancellationToken);

            // TODO: Fix this by including the repo type in the docker container.
            IGitProvider gitProvider =
                _gitProviderFactory.CreateProvider(GitProvider.AzureRepos);
            
            await gitProvider.PostExpiredContainerMessageAsync(
                expiredContainer.InternalBuildId,
                expiredContainer.PullRequestId,
                cancellationToken);
        }
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