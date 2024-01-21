using FluentValidation;
using FluentValidation.Results;
using PreviewEnvironments.Application.Models;
using PreviewEnvironments.Application.Validators;

namespace PreviewEnvironments.Application.Test.Unit.Validators;

public class ApplicationConfigurationValidatorTests
{
    private const string AzureDevOpsPropertyName =
        nameof(ApplicationConfiguration.AzureDevOps);
    
    private const string DockerPropertyName =
        nameof(ApplicationConfiguration.Docker);
    
    private readonly IValidator<ApplicationConfiguration> _validator =
        new ApplicationConfigurationValidator();

    [Fact]
    public void Validator_Should_Pass_For_Valid_Configuration()
    {
        // Arrange
        ApplicationConfiguration configuration = GetValidConfiguration();

        // Act
        ValidationResult result = _validator.Validate(configuration);

        // Assert
        result.IsValid.Should().BeTrue();
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("  ")]
    [InlineData("amqp")]
    public void Validator_Should_Fail_On_Invalid_Scheme(string? scheme)
    {
        // Arrange
        ApplicationConfiguration configuration = GetValidConfiguration();
        
        configuration.Scheme = scheme!;

        // Act
        ValidationResult result = _validator.Validate(configuration);

        // Assert
        using (new AssertionScope())
        {
            result.Errors.Should().HaveCount(1);
            
            result.Errors.Single()
                .PropertyName
                .Should()
                .Be(nameof(ApplicationConfiguration.Scheme));
        }
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("  ")]
    public void Validator_Should_Fail_On_Invalid_Host(string? host)
    {
        // Arrange
        ApplicationConfiguration configuration = GetValidConfiguration();
        
        configuration.Host = host!;

        // Act
        ValidationResult result = _validator.Validate(configuration);

        // Assert
        using (new AssertionScope())
        {
            result.Errors.Should().HaveCount(1);
            
            result.Errors.Single()
                .PropertyName
                .Should()
                .Be(nameof(ApplicationConfiguration.Host));
        }
    }
    
    [Fact]
    public void Validator_Should_Fail_On_Invalid_Container_Timeout_Interval()
    {
        // Arrange
        ApplicationConfiguration configuration = GetValidConfiguration();
        
        configuration.ContainerTimeoutIntervalSeconds = -1;

        // Act
        ValidationResult result = _validator.Validate(configuration);

        // Assert
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Be(nameof(ApplicationConfiguration.ContainerTimeoutIntervalSeconds));
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("  ")]
    [InlineData("amqp")]
    public void Validator_Should_Fail_On_Invalid_DevOps_Scheme(string? scheme)
    {
        // Arrange
        const string expectedPropertyName =
            $"{AzureDevOpsPropertyName}.{nameof(AzureDevOpsConfiguration.Scheme)}";
        
        ApplicationConfiguration configuration = GetValidConfiguration();
        
        configuration.AzureDevOps.Scheme = scheme!;

        // Act
        ValidationResult result = _validator.Validate(configuration);

        // Assert
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Be(expectedPropertyName);
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("  ")]
    public void Validator_Should_Fail_On_Invalid_DevOps_Host(string? host)
    {
        // Arrange
        const string expectedPropertyName =
            $"{AzureDevOpsPropertyName}.{nameof(AzureDevOpsConfiguration.Host)}";
        
        ApplicationConfiguration configuration = GetValidConfiguration();
        
        configuration.AzureDevOps.Host = host!;

        // Act
        ValidationResult result = _validator.Validate(configuration);

        // Assert
        using (new AssertionScope())
        {
            result.Errors.Should().HaveCount(1);
            result.Errors.Single().PropertyName.Should().Be(expectedPropertyName);
        }
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("  ")]
    public void Validator_Should_Fail_On_Invalid_DevOps_Organization(
        string? organization)
    {
        // Arrange
        const string expectedPropertyName =
            $"{AzureDevOpsPropertyName}.{nameof(AzureDevOpsConfiguration.Organization)}";
        
        ApplicationConfiguration configuration = GetValidConfiguration();
        
        configuration.AzureDevOps.Organization = organization!;

        // Act
        ValidationResult result = _validator.Validate(configuration);

        // Assert
        using (new AssertionScope())
        {
            result.Errors.Should().HaveCount(1);
            result.Errors.Single().PropertyName.Should().Be(expectedPropertyName);
        }
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("  ")]
    public void Validator_Should_Fail_On_Invalid_DevOps_Project_Name(
        string? projectName)
    {
        // Arrange
        const string expectedPropertyName =
            $"{AzureDevOpsPropertyName}.{nameof(AzureDevOpsConfiguration.ProjectName)}";
        
        ApplicationConfiguration configuration = GetValidConfiguration();
        
        configuration.AzureDevOps.ProjectName = projectName!;

        // Act
        ValidationResult result = _validator.Validate(configuration);

        // Assert
        using (new AssertionScope())
        {
            result.Errors.Should().HaveCount(1);
            result.Errors.Single().PropertyName.Should().Be(expectedPropertyName);
        }
    }
    
    [Fact]
    public void Validator_Should_Fail_On_Invalid_DevOps_Repository_Id()
    {
        // Arrange
        const string expectedPropertyName =
            $"{AzureDevOpsPropertyName}.{nameof(AzureDevOpsConfiguration.RepositoryId)}";
        
        ApplicationConfiguration configuration = GetValidConfiguration();
        
        configuration.AzureDevOps.RepositoryId = Guid.Empty;

        // Act
        ValidationResult result = _validator.Validate(configuration);

        // Assert
        using (new AssertionScope())
        {
            result.Errors.Should().HaveCount(1);
            result.Errors.Single().PropertyName.Should().Be(expectedPropertyName);
        }
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("  ")]
    public void Validator_Should_Fail_On_Invalid_DevOps_Az_Access_Token(
        string? azAccessToken)
    {
        // Arrange
        const string expectedPropertyName =
            $"{AzureDevOpsPropertyName}.{nameof(AzureDevOpsConfiguration.AzAccessToken)}";
        
        ApplicationConfiguration configuration = GetValidConfiguration();
        
        configuration.AzureDevOps.AzAccessToken = azAccessToken!;

        // Act
        ValidationResult result = _validator.Validate(configuration);

        // Assert
        using (new AssertionScope())
        {
            result.Errors.Should().HaveCount(1);
            result.Errors.Single().PropertyName.Should().Be(expectedPropertyName);
        }
    }
    
    [Fact]
    public void Validator_Should_Fail_On_Invalid_Docker_Create_Container_Retry_Count()
    {
        // Arrange
        const string expectedPropertyName =
            $"{DockerPropertyName}.{nameof(DockerConfiguration.CreateContainerRetryCount)}";
        
        ApplicationConfiguration configuration = GetValidConfiguration();
        
        configuration.Docker.CreateContainerRetryCount = -1;

        // Act
        ValidationResult result = _validator.Validate(configuration);

        // Assert
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Be(expectedPropertyName);
    }
    
    [Fact]
    public void Validator_Should_Fail_On_Invalid_Docker_Container_Timeout_Seconds()
    {
        // Arrange
        const string expectedPropertyName =
            $"{DockerPropertyName}.{nameof(DockerConfiguration.ContainerTimeoutSeconds)}";
        
        ApplicationConfiguration configuration = GetValidConfiguration();
        
        configuration.Docker.ContainerTimeoutSeconds = -1;

        // Act
        ValidationResult result = _validator.Validate(configuration);

        // Assert
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Be(expectedPropertyName);
    }

    private static ApplicationConfiguration GetValidConfiguration()
    {
        return new ApplicationConfiguration
        {
            Scheme = "https",
            Host = "www.preview-environments.com",
            ContainerTimeoutIntervalSeconds = 10,
            AzureDevOps = new AzureDevOpsConfiguration
            {
                Scheme = "https",
                Host = "dev.azure.com",
                Organization = "Test Organization",
                ProjectName = "Test Project Name",
                RepositoryId = Guid.NewGuid(),
                AzAccessToken = "my-access-token"
            },
            Docker = new DockerConfiguration
            {
                ContainerTimeoutSeconds = 10,
                CreateContainerRetryCount = 3
            }
        };
    }
}