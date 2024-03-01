using Microsoft.Extensions.Logging;
using PreviewEnvironments.Application.Features;
using PreviewEnvironments.Application.Features.Abstractions;
using PreviewEnvironments.Application.Models.AzureDevOps.PullRequests;
using PreviewEnvironments.Application.Models.Docker;
using PreviewEnvironments.Application.Services.Abstractions;

namespace PreviewEnvironments.Application.Test.Unit.Features;

public class PullRequestUpdatedFeatureTests
{
    private readonly IPullRequestUpdatedFeature _sut;
    private readonly IDockerService _dockerService;
    private readonly IContainerTracker _containers;

    public PullRequestUpdatedFeatureTests()
    {
        _dockerService = Substitute.For<IDockerService>();
        _containers = Substitute.For<IContainerTracker>();

        _sut = new PullRequestUpdatedFeature(
            Substitute.For<ILogger<PullRequestUpdatedFeature>>(),
            _dockerService,
            _containers);
    }
    
    [Fact]
    public async Task PullRequestUpdated_Should_Return_Early_When_Pull_Request_State_Is_Invalid()
    {
        // Arrange
        PullRequestUpdated pullRequestUpdated = new()
        {
            State = PullRequestState.Active
        };

        // Act
        await _sut.PullRequestUpdatedAsync(pullRequestUpdated);

        // Assert
        await _dockerService
            .Received(0)
            .StopAndRemoveContainerAsync(Arg.Any<string>());
    }
    
    [Fact]
    public async Task PullRequestUpdated_Should_Return_Early_When_Container_Id_Is_Not_Found()
    {
        // Arrange
        PullRequestUpdated pullRequestUpdated = new()
        {
            State = PullRequestState.Completed
        };

        // Act
        await _sut.PullRequestUpdatedAsync(pullRequestUpdated);

        // Assert
        await _dockerService
            .Received(0)
            .StopAndRemoveContainerAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task PullRequestUpdated_Should_Stop_And_Remove_The_Container()
    {
        // Arrange
        const string containerId = "containerId";

        PullRequestUpdated pullRequestUpdated = new()
        {
            Id = 1,
            State = PullRequestState.Completed
        };

        _containers
            .SingleOrDefault(Arg.Any<Predicate<DockerContainer>>())
            .Returns(new DockerContainer
            {
                ContainerId = containerId,
                ImageName = string.Empty,
                ImageTag = string.Empty
            });

        // Act
        await _sut.PullRequestUpdatedAsync(pullRequestUpdated);

        // Assert
        await _dockerService
            .Received(1)
            .StopAndRemoveContainerAsync(containerId);
    }
}