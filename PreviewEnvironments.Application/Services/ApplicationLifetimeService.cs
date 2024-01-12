using PreviewEnvironments.Application.Services.Abstractions;

namespace PreviewEnvironments.Application.Services;

public sealed class ApplicationLifetimeService : IApplicationLifetimeService
{
    private readonly IDockerService _dockerService;

    public ApplicationLifetimeService(IDockerService dockerService)
    {
        _dockerService = dockerService;
    }

    /// <inheritdoc />
    public async Task InitialiseAsync(CancellationToken cancellationToken = default)
    {
        _ = await _dockerService.InitialiseAsync(cancellationToken);
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        return _dockerService.DisposeAsync();
    }
}
