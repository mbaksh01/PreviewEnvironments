using Microsoft.Extensions.Options;
using PreviewEnvironments.Application.Models;
using PreviewEnvironments.Application.Services.Abstractions;

namespace PreviewEnvironments.API.Hosting;

public class ContainerCleanUpBackgroundService : BackgroundService
{
    private readonly IPreviewEnvironmentManager _previewEnvironmentManager;
    private readonly ApplicationConfiguration _configuration;

    public ContainerCleanUpBackgroundService(
        IPreviewEnvironmentManager previewEnvironmentManager,
        IOptions<ApplicationConfiguration> options)
    {
        _previewEnvironmentManager = previewEnvironmentManager;
        _configuration = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(_configuration.ContainerTimeoutIntervalSeconds), stoppingToken);
            await _previewEnvironmentManager.ExpireContainersAsync(stoppingToken);
        }
    }
}
