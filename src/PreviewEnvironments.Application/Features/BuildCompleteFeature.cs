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
    private readonly IRedirectService _redirectService;

    public BuildCompleteFeature(
        ILogger<BuildCompleteFeature> logger,
        IGitProviderFactory gitProviderFactory,
        IDockerService dockerService,
        IContainerTracker containers,
        IConfigurationManager configurationManager,
        IRedirectService redirectService)
    {
        _logger = logger;
        _gitProviderFactory = gitProviderFactory;
        _dockerService = dockerService;
        _containers = containers;
        _configurationManager = configurationManager;
        _redirectService = redirectService;
    }

    /// <inheritdoc />
    public async Task<string?> BuildCompleteAsync(
        BuildComplete buildComplete,
        CancellationToken cancellationToken = default)
    {
        if (!IsBuildValid(buildComplete))
        {
            return null;
        }

        PreviewEnvironmentConfiguration? configuration =
            _configurationManager.GetConfigurationById(buildComplete.InternalBuildId);

        if (configuration is null)
        {
            Log.PreviewEnvironmentConfigurationNotFound(_logger, buildComplete.InternalBuildId);
            return null;
        }

        IGitProvider gitProvider = _gitProviderFactory.CreateProvider(
            configuration.GitProvider.GetGitProviderFromString());

        if (!await IsPullRequestActive(buildComplete, gitProvider, cancellationToken))
        {
            return null;
        }

        try
        {
            await PostPullRequestStatusAsync(
                buildComplete,
                gitProvider,
                PullRequestStatusState.Pending,
                cancellationToken);

            int port = GetDeploymentPort(buildComplete, configuration);

            (DockerContainer? existingContainer, DockerContainer? newContainer, port) = await RunContainer(
                buildComplete,
                configuration,
                port,
                cancellationToken);

            if (newContainer is null)
            {
                throw new Exception("Failed to start container.");
            }

            if (existingContainer is not null)
            {
                _ = _containers.Remove(existingContainer.ContainerId);
            }
            
            Uri containerAddress = new(
                $"{configuration.Deployment.ContainerHostAddress}:{port}");

            string smallId = newContainer.ContainerId[..12];
            
            _containers.Add(newContainer.ContainerId, newContainer);
            
            containerAddress = _redirectService.Add(
                smallId,
                containerAddress,
                buildComplete.Host);
            
            await gitProvider.PostPreviewAvailableMessageAsync(
                buildComplete.InternalBuildId,
                buildComplete.PullRequestId,
                containerAddress,
                cancellationToken);

            await PostPullRequestStatusAsync(
                buildComplete,
                gitProvider,
                PullRequestStatusState.Succeeded,
                cancellationToken);
            
            return smallId;
        }
        catch (Exception ex)
        {
            Log.ErrorProcessingBuildCompleteMessage(_logger, ex);

            await PostPullRequestStatusAsync(
                buildComplete,
                gitProvider,
                PullRequestStatusState.Failed,
                cancellationToken);

            return null;
        }
    }

    private async Task<(DockerContainer? existingContainer, DockerContainer? newContainer, int port)> RunContainer(
        BuildComplete buildComplete,
        PreviewEnvironmentConfiguration configuration,
        int port,
        CancellationToken cancellationToken)
    {
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
                startContainer: !configuration.Deployment.ColdStartEnabled,
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

        return (existingContainer, newContainer, port);
    }

    private int GetDeploymentPort(
        BuildComplete buildComplete,
        PreviewEnvironmentConfiguration configuration)
    {
        int port;
        
        if (configuration.Deployment.AllowedDeploymentPorts.Length == 0)
        {
            port = Random.Shared.Next(10_000, 60_000);
        }
        else
        {
            int[] takenPorts = _containers
                .Where(c => c.InternalBuildId == buildComplete.InternalBuildId)
                .Select(c => c.Port)
                .ToArray();
            
            port = configuration
                .Deployment
                .AllowedDeploymentPorts
                .FirstOrDefault(p => takenPorts.Contains(p) == false);

            if (port != 0)
            {
                return port;
            }
            
            Log.NoAvailablePorts(_logger, buildComplete.InternalBuildId);
            throw new Exception("No free port found to deploy container.");
        }

        return port;
    }

    private static async Task PostPullRequestStatusAsync(
        BuildComplete buildComplete,
        IGitProvider gitProvider,
        PullRequestStatusState state,
        CancellationToken cancellationToken)
    {
        await gitProvider.PostPullRequestStatusAsync(
            buildComplete.InternalBuildId,
            buildComplete.PullRequestId,
            state,
            cancellationToken);
    }

    private async Task<bool> IsPullRequestActive(
        BuildComplete buildComplete,
        IGitProvider gitProvider,
        CancellationToken cancellationToken)
    {
        PullRequestResponse? pullRequest =
            await gitProvider.GetPullRequestById(
                buildComplete.InternalBuildId,
                buildComplete.PullRequestId,
                cancellationToken);
        
        if (pullRequest is null)
        {
            Log.PullRequestNotFound(_logger, buildComplete.PullRequestId);
            return false;
        }
        
        if (pullRequest.Status is not "active")
        {
            Log.BuildCompleteInvalidPullRequestState(_logger, GetPullRequestState(pullRequest.Status));
            return false;
        }

        return true;
    }

    private bool IsBuildValid(BuildComplete buildComplete)
    {
        if (buildComplete.SourceBranch.StartsWith("refs/pull") is false)
        {
            Log.InvalidSourceBranch(_logger, buildComplete.SourceBranch);
            return false;
        }

        if (buildComplete.BuildStatus is BuildStatus.Failed)
        {
            Log.InvalidBuildStatus(_logger, buildComplete.BuildStatus);
            return false;
        }

        return true;
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