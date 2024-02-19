using FluentValidation;
using FluentValidation.Results;
using PreviewEnvironments.Application.Models;
using PreviewEnvironments.Application.Validators;

namespace PreviewEnvironments.Application.Test.Unit.Validators;

public class PreviewEnvironmentConfigurationValidatorTests
{
    private readonly IValidator<PreviewEnvironmentConfiguration> _sut =
        new PreviewEnvironmentConfigurationValidator();

    [Fact]
    public void GitProvider_Validation_Should_Fail_When_Empty()
    {
        // Arrange
        PreviewEnvironmentConfiguration configuration = new()
        {
            GitProvider = string.Empty
        };

        // Act
        ValidationResult result = _sut.Validate(configuration);

        // Assert
        result.IsValid.Should().BeFalse();

        ValidationFailure? error =
            result.Errors.FirstOrDefault(e => e.PropertyName is nameof(PreviewEnvironmentConfiguration.GitProvider));

        error.Should().NotBeNull();
        error!.ErrorMessage.Should().ContainAll("not", "empty");
    }
    
    [Fact]
    public void GitProvider_Validation_Should_Fail_When_Unknown()
    {
        // Arrange
        PreviewEnvironmentConfiguration configuration = new()
        {
            GitProvider = "Unknown"
        };

        // Act
        ValidationResult result = _sut.Validate(configuration);

        // Assert
        result.IsValid.Should().BeFalse();

        ValidationFailure? error =
            result.Errors.FirstOrDefault(e => e.PropertyName is nameof(PreviewEnvironmentConfiguration.GitProvider));

        error.Should().NotBeNull();
        error!.ErrorMessage.Should().ContainAll("not", "Unknown");
    }
    
    [Fact]
    public void BuildServer_Validation_Should_Fail_When_Empty()
    {
        // Arrange
        PreviewEnvironmentConfiguration configuration = new()
        {
            BuildServer = string.Empty
        };

        // Act
        ValidationResult result = _sut.Validate(configuration);

        // Assert
        result.IsValid.Should().BeFalse();

        ValidationFailure? error =
            result.Errors.FirstOrDefault(e => e.PropertyName is nameof(PreviewEnvironmentConfiguration.BuildServer));

        error.Should().NotBeNull();
        error!.ErrorMessage.Should().ContainAll("not", "empty");
    }
    
    [Fact]
    public void BuildServer_Validation_Should_Fail_When_Unknown()
    {
        // Arrange
        PreviewEnvironmentConfiguration configuration = new()
        {
            BuildServer = "Unknown"
        };

        // Act
        ValidationResult result = _sut.Validate(configuration);

        // Assert
        result.IsValid.Should().BeFalse();

        ValidationFailure? error =
            result.Errors.FirstOrDefault(e => e.PropertyName is nameof(PreviewEnvironmentConfiguration.BuildServer));

        error.Should().NotBeNull();
        error!.ErrorMessage.Should().ContainAll("not", "Unknown");
    }
    
    [Fact]
    public void Deployment_Validation_Should_Fail_When_Null()
    {
        // Arrange
        PreviewEnvironmentConfiguration configuration = new()
        {
            Deployment = null!
        };

        // Act
        ValidationResult result = _sut.Validate(configuration);

        // Assert
        result.IsValid.Should().BeFalse();

        ValidationFailure? error =
            result.Errors.FirstOrDefault(e => e.PropertyName is nameof(PreviewEnvironmentConfiguration.Deployment));

        error.Should().NotBeNull();
        error!.ErrorMessage.Should().ContainAll("not", "empty");
    }
    
    [Fact]
    public void AzureRepos_Validation_Should_Fail_When_GitProvider_Is_AzureRepos_And_AzureRepos_Is_Null()
    {
        // Arrange
        PreviewEnvironmentConfiguration configuration = new()
        {
            GitProvider = Constants.GitProviders.AzureRepos,
            AzureRepos = null
        };

        // Act
        ValidationResult result = _sut.Validate(configuration);

        // Assert
        result.IsValid.Should().BeFalse();

        ValidationFailure? error =
            result.Errors.FirstOrDefault(e => e.PropertyName is nameof(PreviewEnvironmentConfiguration.AzureRepos));

        error.Should().NotBeNull();
        error!.ErrorMessage.Should().ContainAll("not", "empty");
    }
    
    [Fact]
    public void AzureRepos_Validation_Should_Not_Fail_When_Value_Is_Null_And_GitProvider_Is_Not_AzureRepos()
    {
        // Arrange
        PreviewEnvironmentConfiguration configuration = new()
        {
            GitProvider = "TestGitProvider",
            AzureRepos = null
        };

        // Act
        ValidationResult result = _sut.Validate(configuration);

        // Assert
        result.IsValid.Should().BeFalse();

        ValidationFailure? error =
            result.Errors.FirstOrDefault(e => e.PropertyName is nameof(PreviewEnvironmentConfiguration.AzureRepos));

        error.Should().BeNull();
    }
    
    [Fact]
    public void AzurePipelines_Validation_Should_Fail_When_GitProvider_Is_AzureRepos_And_AzureRepos_Is_Null()
    {
        // Arrange
        PreviewEnvironmentConfiguration configuration = new()
        {
            BuildServer = Constants.BuildServers.AzurePipelines,
            AzurePipelines = null
        };

        // Act
        ValidationResult result = _sut.Validate(configuration);

        // Assert
        result.IsValid.Should().BeFalse();

        ValidationFailure? error =
            result.Errors.FirstOrDefault(e => e.PropertyName is nameof(PreviewEnvironmentConfiguration.AzurePipelines));

        error.Should().NotBeNull();
        error!.ErrorMessage.Should().ContainAll("not", "empty");
    }
    
    [Fact]
    public void AzurePipelines_Validation_Should_Not_Fail_When_Value_Is_Null_And_GitProvider_Is_Not_AzureRepos()
    {
        // Arrange
        PreviewEnvironmentConfiguration configuration = new()
        {
            BuildServer = "TestBuildServer",
            AzureRepos = null
        };

        // Act
        ValidationResult result = _sut.Validate(configuration);

        // Assert
        result.IsValid.Should().BeFalse();

        ValidationFailure? error =
            result.Errors.FirstOrDefault(e => e.PropertyName is nameof(PreviewEnvironmentConfiguration.AzurePipelines));

        error.Should().BeNull();
    }

    [Fact]
    public void Deployment_ContainerHostAddress_Validation_Should_Fail_When_Empty()
    {
        // Arrange
        PreviewEnvironmentConfiguration configuration = new()
        {
            Deployment = new Deployment
            {
                ContainerHostAddress = string.Empty,
            },
        };

        const string expectedPropertyName =
            $"{nameof(Deployment)}.{nameof(Deployment.ContainerHostAddress)}";

        // Act
        ValidationResult result = _sut.Validate(configuration);

        // Assert
        result.IsValid.Should().BeFalse();

        ValidationFailure? error =
            result.Errors.FirstOrDefault(e => e.PropertyName == expectedPropertyName);

        error.Should().NotBeNull();
        error!.ErrorMessage.Should().ContainAll("not", "empty");
    }
    
    [Fact]
    public void Deployment_ImageName_Validation_Should_Fail_When_Empty()
    {
        // Arrange
        PreviewEnvironmentConfiguration configuration = new()
        {
            Deployment = new Deployment
            {
                ImageName = string.Empty,
            },
        };

        const string expectedPropertyName =
            $"{nameof(Deployment)}.{nameof(Deployment.ImageName)}";

        // Act
        ValidationResult result = _sut.Validate(configuration);

        // Assert
        result.IsValid.Should().BeFalse();

        ValidationFailure? error =
            result.Errors.FirstOrDefault(e => e.PropertyName == expectedPropertyName);

        error.Should().NotBeNull();
        error!.ErrorMessage.Should().ContainAll("not", "empty");
    }
    
    [Fact]
    public void Deployment_ImageRegistry_Validation_Should_Fail_When_Empty()
    {
        // Arrange
        PreviewEnvironmentConfiguration configuration = new()
        {
            Deployment = new Deployment
            {
                ImageRegistry = string.Empty,
            },
        };

        const string expectedPropertyName =
            $"{nameof(Deployment)}.{nameof(Deployment.ImageRegistry)}";

        // Act
        ValidationResult result = _sut.Validate(configuration);

        // Assert
        result.IsValid.Should().BeFalse();

        ValidationFailure? error =
            result.Errors.FirstOrDefault(e => e.PropertyName == expectedPropertyName);

        error.Should().NotBeNull();
        error!.ErrorMessage.Should().ContainAll("not", "empty");
    }

    [Fact]
    public void Deployment_AllowedDeploymentPorts_Validation_Should_Fail_When_List_Is_Not_Unique()
    {
        // Arrange
        PreviewEnvironmentConfiguration configuration = new()
        {
            Deployment = new Deployment
            {
                AllowedDeploymentPorts = [ 1000, 1000 ],
            },
        };

        const string expectedPropertyName =
            $"{nameof(Deployment)}.{nameof(Deployment.AllowedDeploymentPorts)}";

        // Act
        ValidationResult result = _sut.Validate(configuration);

        // Assert
        result.IsValid.Should().BeFalse();

        ValidationFailure? error = result.Errors.FirstOrDefault(
            e => e.PropertyName == expectedPropertyName);

        error.Should().NotBeNull();
        error!.ErrorMessage.Should().ContainAll("must", "be", "unique");
    }
    
    [Fact]
    public void Deployment_ContainerTimeoutSeconds_Validation_Should_Fail_When_Less_Than_Zero()
    {
        // Arrange
        PreviewEnvironmentConfiguration configuration = new()
        {
            Deployment = new Deployment
            {
                ContainerTimeoutSeconds = -1,
            },
        };

        const string expectedPropertyName =
            $"{nameof(Deployment)}.{nameof(Deployment.ContainerTimeoutSeconds)}";

        // Act
        ValidationResult result = _sut.Validate(configuration);

        // Assert
        result.IsValid.Should().BeFalse();

        ValidationFailure? error = result.Errors.FirstOrDefault(
            e => e.PropertyName == expectedPropertyName);

        error.Should().NotBeNull();
        error!.ErrorMessage
            .Should()
            .ContainAll("greater", "than")
            .And
            .ContainAny("zero", "0");
    }
    
    [Fact]
    public void Deployment_CreateContainerRetryCount_Validation_Should_Fail_When_Less_Than_Zero()
    {
        // Arrange
        PreviewEnvironmentConfiguration configuration = new()
        {
            Deployment = new Deployment
            {
                CreateContainerRetryCount = -1,
            },
        };

        const string expectedPropertyName =
            $"{nameof(Deployment)}.{nameof(Deployment.CreateContainerRetryCount)}";

        // Act
        ValidationResult result = _sut.Validate(configuration);

        // Assert
        result.IsValid.Should().BeFalse();

        ValidationFailure? error = result.Errors.FirstOrDefault(
            e => e.PropertyName == expectedPropertyName);

        error.Should().NotBeNull();
        error!.ErrorMessage
            .Should()
            .ContainAll("greater", "than")
            .And
            .ContainAny("zero", "0");
    }

    [Fact]
    public void AzureRepos_BaseAddress_Validation_Should_Fail_When_Null()
    {
        // Arrange
        PreviewEnvironmentConfiguration configuration = new()
        {
            GitProvider = Constants.GitProviders.AzureRepos,
            AzureRepos = new AzureRepos
            {
                BaseAddress = null!,
            }
        };
        
        const string expectedPropertyName =
            $"{nameof(AzureRepos)}.{nameof(AzureRepos.BaseAddress)}";

        // Act
        ValidationResult result = _sut.Validate(configuration);

        // Assert
        result.IsValid.Should().BeFalse();

        ValidationFailure? error = result.Errors.FirstOrDefault(
            e => e.PropertyName == expectedPropertyName);

        error.Should().NotBeNull();
        error!.ErrorMessage.Should().ContainAll("not", "empty");
    }
    
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void AzureRepos_OrganizationName_Validation_Should_Fail_When(string organizationName)
    {
        // Arrange
        PreviewEnvironmentConfiguration configuration = new()
        {
            GitProvider = Constants.GitProviders.AzureRepos,
            AzureRepos = new AzureRepos
            {
                OrganizationName = organizationName,
            }
        };
        
        const string expectedPropertyName =
            $"{nameof(AzureRepos)}.{nameof(AzureRepos.OrganizationName)}";

        // Act
        ValidationResult result = _sut.Validate(configuration);

        // Assert
        result.IsValid.Should().BeFalse();

        ValidationFailure? error = result.Errors.FirstOrDefault(
            e => e.PropertyName == expectedPropertyName);

        error.Should().NotBeNull();
        error!.ErrorMessage.Should().ContainAll("not", "empty");
    }
    
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void AzureRepos_ProjectName_Validation_Should_Fail_When(string projectName)
    {
        // Arrange
        PreviewEnvironmentConfiguration configuration = new()
        {
            GitProvider = Constants.GitProviders.AzureRepos,
            AzureRepos = new AzureRepos
            {
                ProjectName = projectName,
            }
        };
        
        const string expectedPropertyName =
            $"{nameof(AzureRepos)}.{nameof(AzureRepos.ProjectName)}";

        // Act
        ValidationResult result = _sut.Validate(configuration);

        // Assert
        result.IsValid.Should().BeFalse();

        ValidationFailure? error = result.Errors.FirstOrDefault(
            e => e.PropertyName == expectedPropertyName);

        error.Should().NotBeNull();
        error!.ErrorMessage.Should().ContainAll("not", "empty");
    }
    
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void AzureRepos_RepositoryName_Validation_Should_Fail_When(string repositoryName)
    {
        // Arrange
        PreviewEnvironmentConfiguration configuration = new()
        {
            GitProvider = Constants.GitProviders.AzureRepos,
            AzureRepos = new AzureRepos
            {
                RepositoryName = repositoryName,
            }
        };
        
        const string expectedPropertyName =
            $"{nameof(AzureRepos)}.{nameof(AzureRepos.RepositoryName)}";

        // Act
        ValidationResult result = _sut.Validate(configuration);

        // Assert
        result.IsValid.Should().BeFalse();

        ValidationFailure? error = result.Errors.FirstOrDefault(
            e => e.PropertyName == expectedPropertyName);

        error.Should().NotBeNull();
        error!.ErrorMessage.Should().ContainAll("not", "empty");
    }
    
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void AzurePipelines_ProjectName_Validation_Should_Fail_When(string projectName)
    {
        // Arrange
        PreviewEnvironmentConfiguration configuration = new()
        {
            BuildServer = Constants.BuildServers.AzurePipelines,
            AzurePipelines = new AzurePipelines
            {
                ProjectName = projectName,
            }
        };
        
        const string expectedPropertyName =
            $"{nameof(AzurePipelines)}.{nameof(AzurePipelines.ProjectName)}";

        // Act
        ValidationResult result = _sut.Validate(configuration);

        // Assert
        result.IsValid.Should().BeFalse();

        ValidationFailure? error = result.Errors.FirstOrDefault(
            e => e.PropertyName == expectedPropertyName);

        error.Should().NotBeNull();
        error!.ErrorMessage.Should().ContainAll("not", "empty");
    }
    
    [Fact]
    public void AzurePipelines_BuildDefinitionId_Validation_Should_Fail_When_Zero()
    {
        // Arrange
        PreviewEnvironmentConfiguration configuration = new()
        {
            BuildServer = Constants.BuildServers.AzurePipelines,
            AzurePipelines = new AzurePipelines
            {
                BuildDefinitionId = 0,
            }
        };
        
        const string expectedPropertyName =
            $"{nameof(AzurePipelines)}.{nameof(AzurePipelines.BuildDefinitionId)}";

        // Act
        ValidationResult result = _sut.Validate(configuration);

        // Assert
        result.IsValid.Should().BeFalse();

        ValidationFailure? error = result.Errors.FirstOrDefault(
            e => e.PropertyName == expectedPropertyName);

        error.Should().NotBeNull();
        error!.ErrorMessage
            .Should()
            .ContainAll("greater", "than")
            .And
            .ContainAny("zero", "0");
    }
}