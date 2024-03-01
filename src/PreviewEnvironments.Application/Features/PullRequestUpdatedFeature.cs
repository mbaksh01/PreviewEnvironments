using Microsoft.Extensions.Logging;
using PreviewEnvironments.Application.Features.Abstractions;
using PreviewEnvironments.Application.Models.AzureDevOps.PullRequests;
using PreviewEnvironments.Application.Services.Abstractions;

namespace PreviewEnvironments.Application.Features;

internal sealed partial class PullRequestUpdatedFeature : IPullRequestUpdatedFeature
{
    private readonly ILogger<PullRequestUpdatedFeature> _logger;
    private readonly IDockerService _dockerService;
    private readonly IContainerTracker _containers;

    public PullRequestUpdatedFeature(
        ILogger<PullRequestUpdatedFeature> logger,
        IDockerService dockerService,
        IContainerTracker containers)
    {
        _logger = logger;
        _dockerService = dockerService;
        _containers = containers;
    }
    
    /// <inheritdoc />
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
}