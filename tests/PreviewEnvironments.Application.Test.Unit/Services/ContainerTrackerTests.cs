using Microsoft.VisualBasic;
using PreviewEnvironments.Application.Models.Docker;
using PreviewEnvironments.Application.Services;
using PreviewEnvironments.Application.Services.Abstractions;

namespace PreviewEnvironments.Application.Test.Unit.Services;

public sealed class ContainerTrackerTests
{
    private readonly IContainerTracker _sut = new ContainerTracker();

    [Fact]
    public void GetKeys_Should_Return_All_Keys()
    {
        // Arrange
        const int keyCount = 5;
        
        for (int i = 0; i < keyCount; i++)
        {
            _sut.Add($"key{i}", new DockerContainer
            {
                ContainerId = string.Empty,
                ImageName = string.Empty,
                ImageTag = string.Empty,
            });
        }

        IEnumerable<string> expectedKeys =
            Enumerable.Range(0, keyCount).Select(i => $"key{i}");

        // Act
        ICollection<string> keys = _sut.GetKeys();

        // Assert
        keys.Should().HaveCount(keyCount);
        keys.Should().Contain(expectedKeys);
    }

    [Fact]
    public void Add_Should_Add_Container_To_Dictionary()
    {
        // Arrange
        const string key = "key";

        // Act
        _sut.Add(key, new DockerContainer
        {
            ContainerId = string.Empty,
            ImageName = string.Empty,
            ImageTag = string.Empty,
        });

        // Assert
        DockerContainer? container = _sut.Remove(key);

        container.Should().NotBeNull();
    }

    [Fact]
    public void Remove_Should_Remove_Container_From_Dictionary()
    {
        // Arrange
        const string key = "key";
        
        _sut.Add(key, new DockerContainer
        {
            ContainerId = string.Empty,
            ImageName = string.Empty,
            ImageTag = string.Empty,
        });

        // Act
        _ = _sut.Remove(key);
        DockerContainer? container = _sut.Remove(key);

        // Assert
        container.Should().BeNull();
    }

    [Fact]
    public void SingleOrDefault_Should_Return_Matching_Container()
    {
        // Arrange
        const string containerId = "containerId";
        
        _sut.Add("key", new DockerContainer
        {
            ContainerId = containerId,
            ImageName = string.Empty,
            ImageTag = string.Empty,
        });

        // Act
        DockerContainer? container =
            _sut.SingleOrDefault(c => c.ContainerId == containerId);

        // Assert
        container.Should().NotBeNull();
        container!.ContainerId.Should().Be(containerId);
    }
    
    [Fact]
    public void SingleOrDefault_Should_Throw_On_More_Than_One_Match()
    {
        // Arrange
        const string containerId = "containerId";
        
        _sut.Add("key1", new DockerContainer
        {
            ContainerId = containerId,
            ImageName = string.Empty,
            ImageTag = string.Empty,
        });
        
        _sut.Add("key2", new DockerContainer
        {
            ContainerId = containerId,
            ImageName = string.Empty,
            ImageTag = string.Empty,
        });

        // Act
        Action action = () =>
            _sut.SingleOrDefault(c => c.ContainerId == containerId);

        // Assert
        action.Should().Throw<InvalidOperationException>();
    }
    
    [Fact]
    public void Where_Should_Return_List_Matching_Predicate()
    {
        // Arrange
        const string containerId = "containerId";
        
        _sut.Add("key1", new DockerContainer
        {
            ContainerId = containerId,
            ImageName = string.Empty,
            ImageTag = string.Empty,
        });
        
        _sut.Add("key2", new DockerContainer
        {
            ContainerId = containerId,
            ImageName = string.Empty,
            ImageTag = string.Empty,
        });

        // Act
        IEnumerable<DockerContainer> containers =
            _sut.Where(c => c.ContainerId == containerId);

        // Assert
        containers.Should().HaveCount(2);
    }
}