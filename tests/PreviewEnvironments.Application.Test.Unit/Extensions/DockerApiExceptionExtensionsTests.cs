using System.Net;
using Docker.DotNet;
using PreviewEnvironments.Application.Extensions;

namespace PreviewEnvironments.Application.Test.Unit.Extensions;

public class DockerApiExceptionExtensionsTests
{
    [Fact]
    public void GetContainerId_Should_Return_ContainerId_When_Found()
    {
        // Arrange
        const string expectedContainerId =
            "ncy8q7two534ncy8q7two534ncy8q7two534ncy8q7two534ncy8q7two534g9sn";
        
        DockerApiException exception = new(
            HttpStatusCode.InternalServerError,
            $"by container \\\"{expectedContainerId}\\\"");

        // Act
        string? actualContainerId = exception.GetContainerId();

        // Assert
        actualContainerId.Should().Be(expectedContainerId);
    }
    
    [Theory]
    [InlineData("by container \\\"ncy8q7two534ncy8q7two534ncy834g9sn\\\"")]
    [InlineData("")]
    public void GetContainerId_Should_Return_Null_When_Not_Found(
        string incorrectMessageFormat)
    {
        // Arrange
        DockerApiException exception = new(
            HttpStatusCode.InternalServerError,
            incorrectMessageFormat);

        // Act
        string? actualContainerId = exception.GetContainerId();

        // Assert
        actualContainerId.Should().BeNull();
    }
}