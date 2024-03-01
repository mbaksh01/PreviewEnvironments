using Microsoft.Extensions.Options;
using PreviewEnvironments.API.Hosting;
using PreviewEnvironments.Application.Features.Abstractions;
using PreviewEnvironments.Application.Models;
using PreviewEnvironments.Application.Services.Abstractions;

namespace PreviewEnvironments.API.Tests.Unit.Hosting;

public class ContainerCleanUpBackgroundServiceTests
{
    private readonly IExpireContainersFeature _expireContainersFeature =
        Substitute.For<IExpireContainersFeature>();
    
    [Fact]
    public async Task ExecuteAsync_Should_Attempt_To_Expire_Containers_Once()
    {
        // Arrange
        TimeSpan delay = TimeSpan.FromMilliseconds(1500);
        CancellationTokenSource cts = new(delay);

        ContainerCleanUpBackgroundService sut = new(
            _expireContainersFeature,
            Options.Create(new ApplicationConfiguration
            {
                ContainerTimeoutIntervalSeconds = 1
            }));
        
        // Act
        await sut.StartAsync(cts.Token);
        await Task.Delay(delay);
        
        // Assert
        await _expireContainersFeature
            .Received(1)
            .ExpireContainersAsync(Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task ExecuteAsync_Should_Attempt_To_Expire_Containers_Five_Times()
    {
        // Arrange
        TimeSpan delay = TimeSpan.FromMilliseconds(5000);
        CancellationTokenSource cts = new(delay);

        ContainerCleanUpBackgroundService sut = new(
            _expireContainersFeature,
            Options.Create(new ApplicationConfiguration
            {
                ContainerTimeoutIntervalSeconds = 1,
            }));
        
        // Act
        await sut.StartAsync(cts.Token);
        await Task.Delay(delay);
        
        // Assert
        await _expireContainersFeature
            .Received(4)
            .ExpireContainersAsync(Arg.Any<CancellationToken>());
    }
}