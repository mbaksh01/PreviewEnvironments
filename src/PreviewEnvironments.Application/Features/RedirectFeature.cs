using Microsoft.Extensions.Logging;
using PreviewEnvironments.Application.Features.Abstractions;
using PreviewEnvironments.Application.Models.Docker;
using PreviewEnvironments.Application.Services.Abstractions;

namespace PreviewEnvironments.Application.Features;

internal sealed partial class RedirectFeature : IRedirectFeature
{
    private readonly IRedirectService _redirectService;
    private readonly IContainerTracker _containerTracker;
    private readonly IDockerService _dockerService;
    private readonly ILogger<RedirectFeature> _logger;

    public RedirectFeature(
        ILogger<RedirectFeature> logger,
        IRedirectService redirectService,
        IContainerTracker containerTracker,
        IDockerService dockerService)
    {
        _logger = logger;
        _redirectService = redirectService;
        _containerTracker = containerTracker;
        _dockerService = dockerService;
    }

    /// <inheritdoc />
    public async Task<Uri?> GetRedirectUriAsync(string id)
    {
        DockerContainer? container = _containerTracker
            .SingleOrDefault(c => c.ContainerId.StartsWith(id));
        
        if (container is null)
        {
            Log.ContainerNotFound(_logger, id);
            return null;
        }

        if (container.Expired)
        {
            Log.StartingContainer(_logger, container.ContainerId);
            container.Expired = !await _dockerService.StartContainerAsync(container.ContainerId);
            container.CreatedTime = DateTimeOffset.UtcNow;
        }
        
        return _redirectService.GetRedirectUri(id);
    }
}