using Docker.DotNet;
using Docker.DotNet.Models;
using PreviewEnvironments.Application.Extensions;

namespace PreviewEnvironments.Application.Test.Unit.Extensions;

public class DockerClientExtensionsTests
{
    private readonly IDockerClient _dockerClient = Substitute.For<IDockerClient>();
    
    [Fact]
    public async Task GetContainerById_Should_Use_Correct_List_Parameters()
    {
        // Arrange
        const string containerId = "containerId";
        
        ContainersListParameters? actualParameters = null;
        
        _dockerClient.Containers.ListContainersAsync(
            Arg.Any<ContainersListParameters>())
            .Returns([])
            .AndDoes(x => actualParameters = x.Arg<ContainersListParameters>());

        // Act
        await _dockerClient.GetContainerById(containerId);

        // Assert
        actualParameters.Should().NotBeNull();
        
        actualParameters!.All.Should().BeTrue();
        actualParameters.Filters.Should().ContainKey("id");
        actualParameters.Filters["id"].Should().ContainKey(containerId);
        actualParameters.Filters["id"][containerId].Should().BeTrue();
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("  ")]
    public async Task GetContainerByName_Should_Throw_When_Container_Id_Is_Null_Or_Whitespace(
        string containerId)
    {
        // Act
        Func<Task> action = () => _dockerClient.GetContainerById(containerId);

        // Assert
        await action.Should().ThrowAsync<ArgumentException>();
    }
    
    [Fact]
    public async Task GetContainerByName_Should_Use_Correct_List_Parameters()
    {
        // Arrange
        const string containerName = "mystifying_jennings";
        
        ContainersListParameters? actualParameters = null;
        
        _dockerClient.Containers.ListContainersAsync(
                Arg.Any<ContainersListParameters>())
            .Returns([])
            .AndDoes(x => actualParameters = x.Arg<ContainersListParameters>());

        // Act
        await _dockerClient.GetContainerByName(containerName);

        // Assert
        actualParameters.Should().NotBeNull();
        
        actualParameters!.All.Should().BeTrue();
        actualParameters.Filters.Should().ContainKey("name");
        actualParameters.Filters["name"].Should().ContainKey(containerName);
        actualParameters.Filters["name"][containerName].Should().BeTrue();
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("  ")]
    public async Task GetContainerByName_Should_Throw_When_Container_Name_Is_Null_Or_Whitespace(
        string containerId)
    {
        // Act
        Func<Task> action = () => _dockerClient.GetContainerByName(containerId);

        // Assert
        await action.Should().ThrowAsync<ArgumentException>();
    }
}