using FluentValidation;
using FluentValidation.Results;
using PreviewEnvironments.Application.Models;
using PreviewEnvironments.Application.Validators;

namespace PreviewEnvironments.Application.Test.Unit.Validators;

public class ApplicationConfigurationValidatorTests
{
    private readonly IValidator<ApplicationConfiguration> _sut =
        new ApplicationConfigurationValidator();

    [Fact]
    public void Validator_Should_Pass_For_Valid_Configuration()
    {
        // Arrange
        ApplicationConfiguration configuration = GetValidConfiguration();

        // Act
        ValidationResult result = _sut.Validate(configuration);

        // Assert
        result.IsValid.Should().BeTrue();
    }
    
    [Fact]
    public void Validator_Should_Fail_For_Invalid_ContainerTimeoutIntervalSeconds()
    {
        // Arrange
        ApplicationConfiguration configuration = GetValidConfiguration();

        configuration.ContainerTimeoutIntervalSeconds = -5;

        // Act
        ValidationResult result = _sut.Validate(configuration);

        // Assert
        result.IsValid.Should().BeFalse();

        result.Errors.Should().HaveCount(1);

        result
            .Errors[0]
            .PropertyName
            .Should()
            .Be(nameof(ApplicationConfiguration.ContainerTimeoutIntervalSeconds));
    }

    private static ApplicationConfiguration GetValidConfiguration()
    {
        return new ApplicationConfiguration
        {
            ConfigurationFolder = "Configurations",
            ContainerTimeoutIntervalSeconds = 30,
        };
    }
}