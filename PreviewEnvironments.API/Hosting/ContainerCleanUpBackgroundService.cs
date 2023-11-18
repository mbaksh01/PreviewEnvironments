using PreviewEnvironments.Application.Services.Abstractions;

namespace PreviewEnvironments.API.Hosting;

public class ContainerCleanUpBackgroundService : BackgroundService
{
    private readonly IDockerService _dockerService;

    public ContainerCleanUpBackgroundService(IDockerService dockerService)
    {
        _dockerService = dockerService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            await _dockerService.ExpireContainersAsync(stoppingToken);
        }
    }
}
