using Microsoft.Extensions.Options;
using PreviewEnvironments.Application.Models;
using PreviewEnvironments.Application.Services.Abstractions;

namespace PreviewEnvironments.API.Hosting;

public class ContainerCleanUpBackgroundService : BackgroundService
{
    private readonly IDockerService _dockerService;
    private readonly ApplicationConfiguration _configuration;

    public ContainerCleanUpBackgroundService(IDockerService dockerService, IOptions<ApplicationConfiguration> options)
    {
        _dockerService = dockerService;
        _configuration = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(_configuration.ContainerTimeoutIntervalSeconds), stoppingToken);
            await _dockerService.ExpireContainersAsync(stoppingToken);
        }
    }
}
