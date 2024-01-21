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

internal sealed partial class PreviewEnvironmentManager : IPreviewEnvironmentManager
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
            Log.InvalidSourceBranch(_logger, buildComplete.SourceBranch);
            return;
        }

        if (buildComplete.BuildStatus is not BuildStatus.Succeeded)
        {
            Log.InvalidBuildStatus(_logger, buildComplete.BuildStatus);
            return;
        }

        if (_validator.Validate(_configuration).IsValid is false)
        {
            Log.InvalidApplicationConfiguration(_logger);
        }

        SupportedBuildDefinition? supportedBuildDefinition = _configuration
            .AzureDevOps
            .SupportedBuildDefinitions
            .FirstOrDefault(sbd => sbd.BuildDefinitionId == buildComplete.BuildDefinitionId);

        if (supportedBuildDefinition is null)
        {
            Log.BuildDefinitionNotFound(_logger, buildComplete.BuildDefinitionId);
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
                    Log.NoAvailablePorts(_logger, supportedBuildDefinition.BuildDefinitionId);
                    throw new Exception("No free port found to deploy container.");
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
                Log.NoContainerLinkedToPr(_logger, buildComplete.PullRequestNumber);
                
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
                Log.ContainerLinkedToPr(_logger, buildComplete.PullRequestNumber);
                
                newContainer = await _dockerService.RestartContainerAsync(
                    existingContainer,
                    cancellationToken: cancellationToken
                );

                port = existingContainer.Port;
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
            Log.ErrorProcessingBuildCompleteMessage(_logger, ex);

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
            Log.InvalidPullRequestState(_logger, pullRequestUpdated.State);
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

        lock (_containers)
        {
            _containers.Remove(containerId, out _);
        }

        Log.PreviewEnvironmentClosed(_logger, pullRequestId);
    }
    
    /// <inheritdoc />
    public async Task ExpireContainersAsync(CancellationToken cancellationToken = default)
    {
        Log.FindingAndStoppingContainers(_logger);
    
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
    
        Log.FoundContainersToExpire(_logger, containers.Length);
    
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