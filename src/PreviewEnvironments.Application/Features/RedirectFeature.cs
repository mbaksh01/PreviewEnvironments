using PreviewEnvironments.Application.Features.Abstractions;
using PreviewEnvironments.Application.Models.Docker;
using PreviewEnvironments.Application.Services.Abstractions;

namespace PreviewEnvironments.Application.Features;

internal sealed class RedirectFeature : IRedirectFeature
{
    private readonly IRedirectService _redirectService;
    private readonly IContainerTracker _containerTracker;
    private readonly IDockerService _dockerService;

    public RedirectFeature(
        IRedirectService redirectService,
        IContainerTracker containerTracker,
        IDockerService dockerService)
    {
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
            return null;
        }

        if (container.Expired)
        {
            await _dockerService.StartContainerAsync(container.ContainerId);
        }
        
        return _redirectService.GetRedirectUri(id);
    }
}