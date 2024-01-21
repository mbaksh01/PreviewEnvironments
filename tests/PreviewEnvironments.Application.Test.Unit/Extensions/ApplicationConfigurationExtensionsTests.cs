using PreviewEnvironments.Application.Extensions;
using PreviewEnvironments.Application.Models;
using PreviewEnvironments.Application.Models.AzureDevOps.PullRequests;

namespace PreviewEnvironments.Application.Test.Unit.Extensions;

public class ApplicationConfigurationExtensionsTests
{
    [Fact]
    public void GetBuildDefinition_Should_Return_Supported_Build_Definition_When_Found()
    {
        // Arrange
        const int buildDefinitionId = 23;
        
        ApplicationConfiguration configuration = new()
        {
            AzureDevOps = new AzureDevOpsConfiguration
            {
                SupportedBuildDefinitions =
                [
                    new SupportedBuildDefinition
                    {
                        BuildDefinitionId = buildDefinitionId,
                    }
                ]
            }
        };

        // Act
        SupportedBuildDefinition? buildDefinition =
            configuration.GetBuildDefinition(buildDefinitionId);

        // Assert
        buildDefinition.Should().NotBeNull();
        buildDefinition!.BuildDefinitionId.Should().Be(buildDefinitionId);
    }
    
    [Fact]
    public void GetBuildDefinition_Should_Return_Null_When_Not_Found()
    {
        // Arrange
        const int buildDefinitionId = 24;
        
        ApplicationConfiguration configuration = new()
        {
            AzureDevOps = new AzureDevOpsConfiguration
            {
                SupportedBuildDefinitions =
                [
                    new SupportedBuildDefinition
                    {
                        BuildDefinitionId = 23,
                    }
                ]
            }
        };

        // Act
        SupportedBuildDefinition? buildDefinition =
            configuration.GetBuildDefinition(buildDefinitionId);

        // Assert
        buildDefinition.Should().BeNull();
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData("test-access-token", "test-access-token")]
    public void CreateAzureDevOpsMessage_Should_Return_A_Correctly_Mapped_Message(
        string? accessToken,
        string expectedAccessToken)
    {
        // Arrange
        const string scheme = "https";
        const string host = "dev.azure.com";
        const string organization = "Test Organization";
        const string projectName = "Test Project Name";
        Guid repositoryId = Guid.NewGuid();
        
        ApplicationConfiguration configuration = new()
        {
            AzureDevOps = new AzureDevOpsConfiguration
            {
                Scheme = scheme,
                Host = host,
                Organization = organization,
                ProjectName = projectName,
                RepositoryId = repositoryId
            }
        };

        // Act
        PullRequestStatusMessage message =
            configuration.CreateAzureDevOpsMessage<PullRequestStatusMessage>(accessToken);

        // Assert
        message.Should().NotBeNull();

        using (new AssertionScope())
        {
            message.Scheme.Should().Be(scheme);
            message.Host.Should().Be(host);
            message.Organization.Should().Be(organization);
            message.Project.Should().Be(projectName);
            message.RepositoryId.Should().Be(repositoryId);
            message.AccessToken.Should().Be(expectedAccessToken);
        }
    }
}