using Microsoft.Extensions.Options;
using PreviewEnvironments.API.Contracts.AzureDevOps.v2;
using PreviewEnvironments.API.Extensions;
using PreviewEnvironments.Application.Models;

namespace PreviewEnvironments.API.Tests.Unit.Extensions;

public class OptionsExtensionsTests
{
    [Fact]
    public void Apply_Should_Correctly_Apply_Contract_To_Configuration()
    {
        // Arrange
        const string projectName = "Test Project";
        Uri projectUri = new("https://dev.azure.com");
        Guid repositoryId = Guid.NewGuid();
        
        IOptions<ApplicationConfiguration> configuration =
            Options.Create(new ApplicationConfiguration());

        BuildCompleteContract contract = new()
        {
            Resource = new BCResource
            {
                Project = new BCProject
                {
                    Name = projectName,
                    Url = projectUri
                },
                Repository = new BCRepository
                {
                    Id = repositoryId
                }
            }
        };

        // Act
        _ = configuration.Apply(contract);

        // Assert
        AzureDevOpsConfiguration azureDevOpsConfiguration =
            configuration.Value.AzureDevOps;

        azureDevOpsConfiguration.Should().NotBeNull();
        
        using (new AssertionScope())
        {
            azureDevOpsConfiguration.ProjectName.Should().Be(projectName);
            azureDevOpsConfiguration.Scheme.Should().Be(projectUri.Scheme);
            azureDevOpsConfiguration.Host.Should().Be(projectUri.Host);
            azureDevOpsConfiguration.RepositoryId.Should().Be(repositoryId);
        }
    }
}