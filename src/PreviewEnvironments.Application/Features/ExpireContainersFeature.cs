using Microsoft.Extensions.Logging;
using PreviewEnvironments.Application.Features.Abstractions;
using PreviewEnvironments.Application.Models;
using PreviewEnvironments.Application.Models.Docker;
using PreviewEnvironments.Application.Services;
using PreviewEnvironments.Application.Services.Abstractions;

namespace PreviewEnvironments.Application.Features;

internal sealed partial class ExpireContainersFeature : IExpireContainersFeature
{
    private readonly ILogger<ExpireContainersFeature> _logger;
    private readonly IDockerService _dockerService;
    private readonly IContainerTracker _containers;
    private readonly IConfigurationManager _configurationManager;
    private readonly IGitProviderFactory _gitProviderFactory;

    public ExpireContainersFeature(
        ILogger<ExpireContainersFeature> logger,
        IDockerService dockerService,
        IContainerTracker containers,
        IConfigurationManager configurationManager,
        IGitProviderFactory gitProviderFactory)
    {
        _logger = logger;
        _dockerService = dockerService;
        _containers = containers;
        _configurationManager = configurationManager;
        _gitProviderFactory = gitProviderFactory;
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
}