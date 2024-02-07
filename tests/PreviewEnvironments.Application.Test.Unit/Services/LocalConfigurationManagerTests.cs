using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PreviewEnvironments.Application.Helpers;
using PreviewEnvironments.Application.Models;
using PreviewEnvironments.Application.Services;
using PreviewEnvironments.Application.Services.Abstractions;

namespace PreviewEnvironments.Application.Test.Unit.Services;

public class LocalConfigurationManagerTests
{
    private const string ConfigurationFolder = "TestData/Configurations";
    
    private readonly IConfigurationManager _configurationManager;

    public LocalConfigurationManagerTests()
    {
        ApplicationConfiguration configuration = new()
        {
            ConfigurationFolder = ConfigurationFolder
        };
        
        _configurationManager = new LocalConfigurationManager(
            Substitute.For<ILogger<LocalConfigurationManager>>(),
            Options.Create(configuration));
    }

    [Fact]
    public async Task LoadConfigurationsAsync_Should_Load_All_Configurations_In_Directory()
    {
        // Arrange
        string internalId1 = IdHelper.GetAzurePipelinesId(new AzurePipelines
        {
            ProjectName = "TestProject1",
            BuildDefinitionId = 1
        });

        string internalId2 = IdHelper.GetAzurePipelinesId(new AzurePipelines
        {
            ProjectName = "TestProject2",
            BuildDefinitionId = 2
        });

        // Act
        await _configurationManager.LoadConfigurationsAsync();

        // Assert
        PreviewEnvironmentConfiguration? configuration1 =
            _configurationManager.GetConfigurationByBuildId(internalId1);

        PreviewEnvironmentConfiguration? configuration2 =
            _configurationManager.GetConfigurationByBuildId(internalId2);

        configuration1.Should().NotBeNull();
        configuration2.Should().NotBeNull();
    }
}