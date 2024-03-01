using System.Diagnostics;
using Microsoft.Extensions.Logging;
using PreviewEnvironments.Application.Extensions;
using PreviewEnvironments.Application.Features.Abstractions;
using PreviewEnvironments.Application.Models;
using PreviewEnvironments.Application.Models.AzureDevOps.Builds;
using PreviewEnvironments.Application.Models.AzureDevOps.Contracts;
using PreviewEnvironments.Application.Models.AzureDevOps.PullRequests;
using PreviewEnvironments.Application.Models.Docker;
using PreviewEnvironments.Application.Services.Abstractions;

namespace PreviewEnvironments.Application.Features;

internal sealed partial class BuildCompleteFeature : IBuildCompleteFeature
{
    private readonly ILogger<BuildCompleteFeature> _logger;
    private readonly IGitProviderFactory _gitProviderFactory;
    private readonly IDockerService _dockerService;
    private readonly IContainerTracker _containers;
    private readonly IConfigurationManager _configurationManager;

    public BuildCompleteFeature(
        ILogger<BuildCompleteFeature> logger,
        IGitProviderFactory gitProviderFactory,
        IDockerService dockerService,
        IContainerTracker containers,
        IConfigurationManager configurationManager)
    {
        _logger = logger;
        _gitProviderFactory = gitProviderFactory;
        _dockerService = dockerService;
        _containers = containers;
        _configurationManager = configurationManager;
    }

    /// <inheritdoc />
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

        PreviewEnvironmentConfiguration? configuration =
            _configurationManager.GetConfigurationById(buildComplete.InternalBuildId);

        if (configuration is null)
        {
            Log.PreviewEnvironmentConfigurationNotFound(_logger, buildComplete.InternalBuildId);
            return;
        }

        IGitProvider gitProvider = _gitProviderFactory.CreateProvider(
            configuration.GitProvider.GetGitProviderFromString());
        
        PullRequestResponse? pullRequest =
            await gitProvider.GetPullRequestById(
                buildComplete.InternalBuildId,
                buildComplete.PullRequestId,
                cancellationToken);
        
        if (pullRequest is null)
        {
            Log.PullRequestNotFound(_logger, buildComplete.PullRequestId);
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
                buildComplete.PullRequestId,
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
                
                takenPorts = _containers
                    .Where(c => c.InternalBuildId == buildComplete.InternalBuildId)
                    .Select(c => c.Port)
                    .ToArray();
            
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

            // ASSUMPTION: Assuming that the tag is the pr number with 'pr-' prefixed.
            DockerContainer? existingContainer = _containers.SingleOrDefault(
                dc =>
                    dc.ImageName == $"{configuration.Deployment.ImageRegistry}/{configuration.Deployment.ImageName.ToLower()}"
                    && dc.ImageTag == $"pr-{buildComplete.PullRequestId}"
            );
            
            DockerContainer? newContainer;

            if (existingContainer is null)
            {
                Log.NoContainerLinkedToPr(_logger, buildComplete.PullRequestId);
                
                newContainer = await _dockerService.RunContainerAsync(
                    configuration.Deployment.ImageName,
                    $"pr-{buildComplete.PullRequestId}",
                    buildComplete.InternalBuildId,
                    port,
                    configuration.Deployment.ImageRegistry,
                    cancellationToken: cancellationToken
                );
            }
            else
            {
                Log.ContainerLinkedToPr(_logger, buildComplete.PullRequestId);
                
                newContainer = await _dockerService.RestartContainerAsync(
                    existingContainer,
                    cancellationToken: cancellationToken
                );

                port = existingContainer.Port;
            }

            if (existingContainer is not null)
            {
                _ = _containers.Remove(existingContainer.ContainerId);
            }
            
            if (newContainer is not null)
            {
                _containers.Add(newContainer.ContainerId, newContainer);
            }

            Uri containerAddress = new(
                $"{configuration.Deployment.ContainerHostAddress}:{port}");
            
            await gitProvider.PostPreviewAvailableMessageAsync(
                buildComplete.InternalBuildId,
                buildComplete.PullRequestId,
                containerAddress,
                cancellationToken);

            await gitProvider.PostPullRequestStatusAsync(
                buildComplete.InternalBuildId,
                buildComplete.PullRequestId,
                PullRequestStatusState.Succeeded,
                cancellationToken);
        }
        catch (Exception ex)
        {
            Log.ErrorProcessingBuildCompleteMessage(_logger, ex);

            await gitProvider.PostPullRequestStatusAsync(
                buildComplete.InternalBuildId,
                buildComplete.PullRequestId,
                PullRequestStatusState.Failed,
                cancellationToken);
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
}