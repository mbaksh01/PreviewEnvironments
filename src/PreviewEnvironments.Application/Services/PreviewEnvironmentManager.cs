using System.Collections.Concurrent;
using System.Diagnostics;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
    private readonly ApplicationConfiguration _configuration;
    private readonly ConcurrentDictionary<string, DockerContainer> _containers;
    
    public PreviewEnvironmentManager(
        ILogger<PreviewEnvironmentManager> logger,
        IValidator<ApplicationConfiguration> validator,
        IOptions<ApplicationConfiguration> configuration,
        IGitProviderFactory gitProviderFactory,
        IDockerService dockerService,
        IConfigurationManager configurationManager)
    {
        _logger = logger;
        _validator = validator;
        _gitProviderFactory = gitProviderFactory;
        _dockerService = dockerService;
        _configurationManager = configurationManager;
        _configuration = configuration.Value;
        _containers = new ConcurrentDictionary<string, DockerContainer>();
    }

    public async Task InitialiseAsync(CancellationToken cancellationToken = default)
    {
        await _configurationManager.LoadConfigurationsAsync(cancellationToken);
        // _configurationManager.ValidateConfigurations();
        await _dockerService.InitialiseAsync(cancellationToken);
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

        PreviewEnvironmentConfiguration? configuration =
            _configurationManager.GetConfigurationByBuildId(buildComplete.InternalBuildId);

        if (configuration is null)
        {
            Log.PreviewEnvironmentConfigurationNotFound(_logger, buildComplete.InternalBuildId);
            return;
        }

        IGitProvider gitProvider = _gitProviderFactory.CreateProvider(
            GetGitProviderFromString(configuration.GitProvider));
        
        PullRequestResponse? pullRequest =
            await gitProvider.GetPullRequestById(
                buildComplete.InternalBuildId,
                buildComplete.PullRequestNumber,
                cancellationToken);
        
        if (pullRequest is null)
        {
            Log.PullRequestNotFound(_logger, buildComplete.PullRequestNumber);
            return;
        }
        
        if (pullRequest.Status is not "active")
        {
            Log.BuildCompleteInvalidPullRequestState(_logger, GetPullRequestState(pullRequest.Status));
            return;
        }

        try
        {
            await gitProvider.PostPullRequestStatusAsync(
                buildComplete.InternalBuildId,
                buildComplete.PullRequestNumber,
                PullRequestStatusState.Pending,
                cancellationToken);
            
            int port;
            
            if (configuration.Deployment.AllowedDeploymentPorts.Length == 0)
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
                        .Where(c => c.InternalBuildId == buildComplete.InternalBuildId)
                        .Select(c => c.Port)
                        .ToArray();
                }
            
                port = configuration
                    .Deployment
                    .AllowedDeploymentPorts
                    .FirstOrDefault(p => takenPorts.Contains(p) == false);
            
                if (port == default)
                {
                    Log.NoAvailablePorts(_logger, buildComplete.InternalBuildId);
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
                        dc.ImageName == $"{configuration.Deployment.ImageRegistry}/{configuration.Deployment.ImageName.ToLower()}"
                        && dc.ImageTag == $"pr-{buildComplete.PullRequestNumber}"
                );
            }

            if (existingContainer is null)
            {
                Log.NoContainerLinkedToPr(_logger, buildComplete.PullRequestNumber);
                
                newContainer = await _dockerService.RunContainerAsync(
                    configuration.Deployment.ImageName,
                    $"pr-{buildComplete.PullRequestNumber}",
                    buildComplete.InternalBuildId,
                    port,
                    configuration.Deployment.ImageRegistry,
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

            Uri containerAddress = new(
                $"{configuration.Deployment.ContainerHostAddress}:{port}");
            
            await gitProvider.PostPreviewAvailableMessageAsync(
                buildComplete.InternalBuildId,
                buildComplete.PullRequestNumber,
                containerAddress,
                cancellationToken);

            await gitProvider.PostPullRequestStatusAsync(
                buildComplete.InternalBuildId,
                buildComplete.PullRequestNumber,
                PullRequestStatusState.Succeeded,
                cancellationToken);
        }
        catch (Exception ex)
        {
            Log.ErrorProcessingBuildCompleteMessage(_logger, ex);

            await gitProvider.PostPullRequestStatusAsync(
                buildComplete.InternalBuildId,
                buildComplete.PullRequestNumber,
                PullRequestStatusState.Failed,
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
            Log.PullRequestUpdatedInvalidPullRequestState(_logger, pullRequestUpdated.State);
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

        List<DockerContainer> expiredContainers = [];

        lock (_containers)
        {
            foreach (DockerContainer container in _containers.Values)
            {
                Deployment? deployment = _configurationManager
                    .GetConfigurationByBuildId(container.InternalBuildId)?.Deployment;

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
        }

        Log.FoundContainersToExpire(_logger, expiredContainers.Count);

        foreach (DockerContainer expiredContainer in expiredContainers)
        {
            expiredContainer.Expired = await _dockerService.StopContainerAsync(
                expiredContainer.ContainerId,
                cancellationToken);

            // TODO: Fix this
            // await _gitProviderFactory.PostExpiredContainerMessageAsync(
            //     expiredContainer.PullRequestId,
            //     cancellationToken);
        }
    }

    private static PullRequestState GetPullRequestState(string state)
    {
        return state switch
        {
            "active" => PullRequestState.Active,
            "abandoned" => PullRequestState.Abandoned,
            "completed" => PullRequestState.Completed,
            _ => throw new UnreachableException()
        };
    }

    private static GitProvider GetGitProviderFromString(string provider)
    {
        return provider switch
        {
            Constants.GitProviders.AzureRepos => GitProvider.AzureRepos,
            _ => throw new NotSupportedException(
                $"The git provider '{provider}' is not supported.")
        };
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