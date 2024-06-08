using Microsoft.Extensions.Logging;
using PreviewEnvironments.Application.Features;
using PreviewEnvironments.Application.Features.Abstractions;
using PreviewEnvironments.Application.Models.Docker;
using PreviewEnvironments.Application.Services.Abstractions;

namespace PreviewEnvironments.Application.Test.Unit.Features;

public class RedirectFeatureTests
{
    private readonly IRedirectFeature _sut;
    private readonly IRedirectService _redirectService;
    private readonly IContainerTracker _containerTracker;
    private readonly IDockerService _dockerService;
    
    public RedirectFeatureTests()
    {
        _redirectService = Substitute.For<IRedirectService>();
        _containerTracker = Substitute.For<IContainerTracker>();
        _dockerService = Substitute.For<IDockerService>();
        
        _sut = new RedirectFeature(
            Substitute.For<ILogger<RedirectFeature>>(),
            _redirectService,
            _containerTracker,
            _dockerService);
    }
    
    [Fact]
    public async Task GetRedirectUriAsync_Should_Return_Correct_Uri()
    {
        // Arrange
        string id = new('A', 12);
        Uri expectedUri = new("https://test.application.com");

        _containerTracker
            .SingleOrDefault(Arg.Any<Predicate<DockerContainer>>())
            .Returns(new DockerContainer
            {
                ContainerId = id,
                ImageName = string.Empty,
                ImageTag = string.Empty
            });
        
        _redirectService
            .GetRedirectUri(id)
            .Returns(expectedUri);

        // Act
        Uri? actualUri = await _sut.GetRedirectUriAsync(id);

        // Assert
        actualUri.Should().BeEquivalentTo(expectedUri);
    }
    
    [Fact]
    public async Task GetRedirectUriAsync_Should_Return_Null_If_Container_Is_Not_Found()
    {
        // Arrange
        string id = new('A', 12);

        // Act
        Uri? actualUri = await _sut.GetRedirectUriAsync(id);

        // Assert
        actualUri.Should().BeNull();
    }
    
    [Fact]
    public async Task GetRedirectUriAsync_Should_Return_Start_Container_When_Expired()
    {
        // Arrange
        string id = new('A', 12);
        bool startContainerResponse = true;
        DockerContainer container = new()
        {
            ContainerId = id,
            ImageName = string.Empty,
            ImageTag = string.Empty,
            Expired = true,
        };
        
        _containerTracker
            .SingleOrDefault(Arg.Any<Predicate<DockerContainer>>())
            .Returns(container);

        _dockerService
            .StartContainerAsync(id)
            .Returns(startContainerResponse);

        // Act
        _ = await _sut.GetRedirectUriAsync(id);

        // Assert
        await _dockerService
            .Received(1)
            .StartContainerAsync(id);

        container.Expired.Should().BeFalse();
        container.CreatedTime.Should().BeCloseTo(DateTimeOffset.UtcNow, precision: TimeSpan.FromSeconds(1));
    }
}