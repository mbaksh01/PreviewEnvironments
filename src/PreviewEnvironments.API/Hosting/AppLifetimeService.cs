using Docker.DotNet.Models;
using PreviewEnvironments.Application.Services;
using PreviewEnvironments.Application.Services.Abstractions;

namespace PreviewEnvironments.API.Hosting;

internal sealed class AppLifetimeService : IHostedLifecycleService
{
    private readonly IPreviewEnvironmentManager _previewEnvironmentManager;

    public AppLifetimeService(IPreviewEnvironmentManager previewEnvironmentManager)
    {
        _previewEnvironmentManager = previewEnvironmentManager;
    }

    public Task StartingAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public async Task StartedAsync(CancellationToken cancellationToken)
    {
        await _previewEnvironmentManager.InitialiseAsync(cancellationToken);
    }

    public Task StoppingAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StoppedAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
