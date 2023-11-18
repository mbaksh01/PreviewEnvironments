using PreviewEnvironments.Application.Services;

namespace PreviewEnvironments.API.Hosting;

internal sealed class AppLifetimeService : IHostedLifecycleService
{
    private readonly ApplicationLifetimeService _applicationLifetimeService;

    public AppLifetimeService(ApplicationLifetimeService applicationLifetimeService)
    {
        _applicationLifetimeService = applicationLifetimeService;
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
        await _applicationLifetimeService.InitialiseAsync(cancellationToken);
    }

    public async Task StoppingAsync(CancellationToken cancellationToken)
    {
        await _applicationLifetimeService.DisposeAsync();
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
