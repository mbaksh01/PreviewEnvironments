using System.Collections.Concurrent;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PreviewEnvironments.Application.Extensions;
using PreviewEnvironments.Application.Models;
using PreviewEnvironments.Application.Models.AzureDevOps;
using PreviewEnvironments.Application.Models.AzureDevOps.Builds;
using PreviewEnvironments.Application.Models.AzureDevOps.PullRequests;
using PreviewEnvironments.Application.Models.Docker;
using PreviewEnvironments.Application.Services.Abstractions;

namespace PreviewEnvironments.Application.Services;

internal sealed class PreviewEnvironmentManager : IPreviewEnvironmentManager
{
    private readonly ILogger<PreviewEnvironmentManager> _logger;
    private readonly IValidator<ApplicationConfiguration> _validator;
    private readonly IAzureDevOpsService _azureDevOpsService;
    private readonly IDockerService _dockerService;
    private readonly ApplicationConfiguration _configuration;
    private readonly ConcurrentDictionary<string, DockerContainer> _containers;
    
    public PreviewEnvironmentManager(
        ILogger<PreviewEnvironmentManager> logger,
        IValidator<ApplicationConfiguration> validator,
        IOptions<ApplicationConfiguration> configuration,
        IAzureDevOpsService azureDevOpsService,
        IDockerService dockerService)
    {
        _logger = logger;
        _validator = validator;
        _azureDevOpsService = azureDevOpsService;
        _dockerService = dockerService;
        _configuration = configuration.Value;
        _containers = new ConcurrentDictionary<string, DockerContainer>();
    }

    public Task InitialiseAsync(CancellationToken cancellationToken = default)
    {
        return _dockerService.InitialiseAsync(cancellationToken);
    }
    
    /// <summary>
    /// Takes a complete build and starts its associated preview environment.
    /// </summary>
    /// <param name="buildComplete">Information about the complete build.</param>
    /// <param name="cancellationToken">
    /// Cancellation token used to stop this task.
    /// </param>
    /// <returns></returns>
    public async Task BuildCompleteAsync(
        BuildComplete buildComplete,
        CancellationToken cancellationToken = default)
    {
        if (buildComplete.SourceBranch.StartsWith("refs/pull") is false)
        {
            return;
        }

        if (buildComplete.BuildStatus is not BuildStatus.Succeeded)
        {
            return;
        }

        if (_validator.Validate(_configuration).IsValid is false)
        {
            _logger.LogWarning("The application configuration was not deemed to be valid. Some parts of the application not may not work as expected.");
        }

        SupportedBuildDefinition? supportedBuildDefinition = _configuration
            .AzureDevOps
            .SupportedBuildDefinitions
            .FirstOrDefault(sbd => sbd.BuildDefinitionId == buildComplete.BuildDefinitionId);

        if (supportedBuildDefinition is null)
        {
            // TODO: log error
            return;
        }

        // TODO: Validate Azure DevOps configuration and guard against invalid config.

        try
        {
            await _azureDevOpsService.PostPullRequestStatusAsync(
                CreateStatusMessage(
                    buildComplete,
                    PullRequestStatusState.Pending),
                cancellationToken);
            
            int port;
            
            if (supportedBuildDefinition.AllowedImagePorts.Length == 0)
            {
                port = Random.Shared.Next(10_000, 60_000);
            }
            else
            {
                int[] takenPorts;
                
                lock (_containers)
                {
                    takenPorts = _containers
                        .Values
                        .Where(c => c.BuildDefinitionId == supportedBuildDefinition.BuildDefinitionId)
                        .Select(c => c.Port)
                        .ToArray();
                }
            
                port = supportedBuildDefinition
                    .AllowedImagePorts
                    .FirstOrDefault(p => takenPorts.Contains(p) == false);
            
                if (port == default)
                {
                    _logger.LogError(
                        "There were no available ports to start this container. Consider increasing the number of allowed ports for build definition '{buildDefinitionId}'",
                        supportedBuildDefinition.BuildDefinitionId);
            
                    return;
                }
            }
            
            DockerContainer? existingContainer;
            DockerContainer? newContainer;

            lock (_containers)
            {
                // ASSUMPTION: Assuming that the tag is the pr number with 'pr-' prefixed.
                existingContainer = _containers.Values.SingleOrDefault(
                    dc =>
                        dc.ImageName == $"{supportedBuildDefinition.DockerRegistry}/{supportedBuildDefinition.ImageName.ToLower()}"
                        && dc.ImageTag == $"pr-{buildComplete.PullRequestNumber}"
                );
            }

            if (existingContainer is null)
            {
                _logger.LogDebug(
                    "Could not find a container linked to the pull request '{pullRequestNumber}'.",
                    buildComplete.PullRequestNumber);
                
                newContainer = await _dockerService.RunContainerAsync(
                    supportedBuildDefinition.ImageName,
                    $"pr-{buildComplete.PullRequestNumber}",
                    supportedBuildDefinition.BuildDefinitionId,
                    port,
                    supportedBuildDefinition.DockerRegistry,
                    cancellationToken: cancellationToken
                );
            }
            else
            {
                _logger.LogDebug(
                    "Found a container linked to pull request '{pullRequestNumber}'",
                    buildComplete.PullRequestNumber);
                
                newContainer = await _dockerService.RestartContainerAsync(
                    existingContainer,
                    cancellationToken: cancellationToken
                );
            }

            lock (_containers)
            {
                if (existingContainer is not null)
                {
                    _containers.Remove(existingContainer.ContainerId, out _);
                }
                
                if (newContainer is not null)
                {
                    _containers.TryAdd(newContainer.ContainerId, newContainer);
                }
            }

            PreviewAvailableMessage message = _configuration
                .CreateAzureDevOpsMessage<PreviewAvailableMessage>();

            message.PullRequestNumber = buildComplete.PullRequestNumber;
            message.PreviewEnvironmentAddress =
                $"{_configuration.Scheme}://{_configuration.Host}:{port}";

            await _azureDevOpsService.PostPreviewAvailableMessageAsync(message, cancellationToken);

            await _azureDevOpsService.PostPullRequestStatusAsync(
                CreateStatusMessage(
                    buildComplete,
                    PullRequestStatusState.Succeeded,
                    port
                ),
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "An error occurred whilst processing a build complete message."
            );

            await _azureDevOpsService.PostPullRequestStatusAsync(
                CreateStatusMessage(
                    buildComplete,
                    PullRequestStatusState.Failed
                ),
                cancellationToken);
        }
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
            return;
        }
        
        int pullRequestId = pullRequestUpdated.Id;

        string? containerId;

        lock (_containers)
        {
            containerId = _containers.Values.SingleOrDefault(c => c.PullRequestId == pullRequestId)?.ContainerId;
        }

        if (string.IsNullOrWhiteSpace(containerId))
        {
            // TODO: log error
            return;
        }
        
        bool response = await _dockerService.StopAndRemoveContainerAsync(
            containerId,
            cancellationToken
        );

        if (response)
        {
            lock (_containers)
            {
                _containers.Remove(containerId, out _);
            }
            
            _logger.LogInformation(
                "Successfully closed preview environment linked to pull request {pullRequestId}.",
                pullRequestId
            );
            
            return;
        }
        
        _logger.LogInformation(
            "Failed to close preview environment linked to pull request {pullRequestId}.",
            pullRequestId
        );
    }
    
    /// <inheritdoc />
    public async Task ExpireContainersAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Attempting to find and stop expired containers.");
    
        DockerContainer[] containers;
    
        lock (_containers)
        {
            TimeSpan timeout = TimeSpan.FromSeconds(_configuration.Docker.ContainerTimeoutSeconds);
            
            containers = _containers
                .Where(c =>
                    c.Value.CreatedTime + timeout < DateTimeOffset.UtcNow
                    && c.Value is { CanExpire: true, Expired: false }
                )
                .Select(c => c.Value)
                .ToArray();
        }
    
        _logger.LogDebug(
            "Found {containerCount} containers to expire.",
            containers.Length
        );
    
        foreach (DockerContainer container in containers)
        {
            container.Expired = await _dockerService.StopContainerAsync(
                container.ContainerId,
                cancellationToken
            );

            await _azureDevOpsService.PostExpiredContainerMessageAsync(
                container.PullRequestId,
                cancellationToken);
        }
    }
    
    /// <summary>
    /// Creates a <see cref="PullRequestStatusMessage"/> from the given
    /// parameters and the current configuration.
    /// </summary>
    /// <param name="buildComplete"></param>
    /// <param name="state">State of the preview environment.</param>
    /// <param name="port">Port the container was started on.</param>
    /// <returns>
    /// A correctly initialised <see cref="PullRequestStatusMessage"/>.
    /// </returns>
    private PullRequestStatusMessage CreateStatusMessage(
        BuildComplete buildComplete,
        PullRequestStatusState state,
        int port = 0)
    {
        PullRequestStatusMessage message = _configuration
            .CreateAzureDevOpsMessage<PullRequestStatusMessage>();

        message.PullRequestNumber = buildComplete.PullRequestNumber;
        message.BuildPipelineAddress = buildComplete.BuildUrl.ToString();
        message.State = state;
        message.Port = port;

        return message;
    }

    public async ValueTask DisposeAsync()
    {
        ICollection<string> containerIds;
        
        lock (_containers)
        {
            containerIds = _containers.Keys;
        }
        
        foreach (string containerId in containerIds)
        {
            await _dockerService.StopAndRemoveContainerAsync(containerId);
        }
        
        _dockerService.Dispose();
    }
}