using FluentValidation;
using FluentValidation.Results;
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

    private readonly IValidator<PreviewEnvironmentConfiguration> _validator =
        Substitute.For<IValidator<PreviewEnvironmentConfiguration>>();

    private readonly IConfigurationManager _sut;

    public LocalConfigurationManagerTests()
    {
        ApplicationConfiguration configuration = new()
        {
            ConfigurationFolder = ConfigurationFolder
        };
        
        _sut = new LocalConfigurationManager(
            Substitute.For<ILogger<LocalConfigurationManager>>(),
            Options.Create(configuration),
            _validator);
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
        await _sut.LoadConfigurationsAsync();

        // Assert
        PreviewEnvironmentConfiguration? configuration1 =
            _sut.GetConfigurationByBuildId(internalId1);

        PreviewEnvironmentConfiguration? configuration2 =
            _sut.GetConfigurationByBuildId(internalId2);

        configuration1.Should().NotBeNull();
        configuration2.Should().NotBeNull();
    }

    [Fact]
    public async Task ValidateConfigurations_Should_Remove_Invalid_Configurations_And_Keep_Valid_Configurations()
    {
        // Arrange
        const string invalidConfigurationProjectName = "TestProject3";
        
        string validConfigurationId = IdHelper.GetAzurePipelinesId(new AzurePipelines
        {
            ProjectName = "TestProject1",
            BuildDefinitionId = 1
        });
        
        string invalidConfigurationId = IdHelper.GetAzurePipelinesId(new AzurePipelines
        {
            ProjectName = invalidConfigurationProjectName,
            BuildDefinitionId = 1
        });
        
        await _sut.LoadConfigurationsAsync();

        _sut.GetConfigurationByBuildId(validConfigurationId).Should().NotBeNull();
        _sut.GetConfigurationByBuildId(invalidConfigurationId).Should().NotBeNull();

        _validator
            .Validate(Arg.Is<PreviewEnvironmentConfiguration>(c =>
                c.AzurePipelines!.ProjectName == invalidConfigurationProjectName))
            .Returns(new ValidationResult([ new ValidationFailure() ]));
        
        _validator
            .Validate(Arg.Is<PreviewEnvironmentConfiguration>(c =>
                c.AzurePipelines!.ProjectName != invalidConfigurationProjectName))
            .Returns(new ValidationResult());
        
        // Act
        _sut.ValidateConfigurations();

        // Assert
        _sut.GetConfigurationByBuildId(validConfigurationId).Should().NotBeNull();
        _sut.GetConfigurationByBuildId(invalidConfigurationId).Should().BeNull();
    }
}