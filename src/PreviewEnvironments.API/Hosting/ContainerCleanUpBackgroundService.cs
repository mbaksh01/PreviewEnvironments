using Microsoft.Extensions.Options;
using PreviewEnvironments.Application.Features.Abstractions;
using PreviewEnvironments.Application.Models;
using PreviewEnvironments.Application.Services.Abstractions;

namespace PreviewEnvironments.API.Hosting;

public class ContainerCleanUpBackgroundService : BackgroundService
{
    private readonly IExpireContainersFeature _expireContainersFeature;
    private readonly ApplicationConfiguration _configuration;

    public ContainerCleanUpBackgroundService(
        IExpireContainersFeature expireContainersFeature,
        IOptions<ApplicationConfiguration> options)
    {
        _expireContainersFeature = expireContainersFeature;
        _configuration = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(_configuration.ContainerTimeoutIntervalSeconds), stoppingToken);
            await _expireContainersFeature.ExpireContainersAsync(stoppingToken);
        }
    }
}
