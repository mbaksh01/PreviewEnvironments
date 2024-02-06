using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PreviewEnvironments.Application.Models;
using PreviewEnvironments.Application.Models.AzureDevOps.Builds;
using PreviewEnvironments.Application.Models.AzureDevOps.Contracts;
using PreviewEnvironments.Application.Models.AzureDevOps.PullRequests;
using PreviewEnvironments.Application.Models.Docker;
using PreviewEnvironments.Application.Services;
using PreviewEnvironments.Application.Services.Abstractions;

namespace PreviewEnvironments.Application.Test.Unit.Services;

public class PreviewEnvironmentManagerTests
{
    private const string TestInternalBuildId = "test-internal-build-id";
    private const string DefaultContainerScheme = "https";
    private const string DefaultContainerHost = "test.domain.com";

    private readonly IPreviewEnvironmentManager _sut;
    private readonly IGitProviderFactory _gitProviderFactory;
    private readonly IGitProvider _gitProvider;
    private readonly IValidator<ApplicationConfiguration> _validator;
    private readonly IOptions<ApplicationConfiguration> _options;
    private readonly IDockerService _dockerService;
    private readonly IConfigurationManager _configurationManager;

    public PreviewEnvironmentManagerTests()
    {
        _gitProviderFactory = Substitute.For<IGitProviderFactory>();
        _gitProvider = Substitute.For<IGitProvider>();
        _validator = Substitute.For<IValidator<ApplicationConfiguration>>();
        _options = Options.Create(new ApplicationConfiguration());
        _dockerService = Substitute.For<IDockerService>();
        _configurationManager = Substitute.For<IConfigurationManager>();

        _sut = new PreviewEnvironmentManager(
            Substitute.For<ILogger<PreviewEnvironmentManager>>(),
            _validator,
            _options,
            _gitProviderFactory,
            _dockerService,
            _configurationManager);

        _gitProviderFactory
            .CreateProvider(Arg.Any<GitProvider>())
            .Returns(_gitProvider);
    }

    [Fact]
    public async Task InitialiseAsync_Should_Initialise_Dependant_Services()
    {
        // Act
        await _sut.InitialiseAsync();

        // Assert
        await _dockerService
            .Received(1)
            .InitialiseAsync();

        await _configurationManager
            .Received(1)
            .LoadConfigurationsAsync();
    }
    
    [Fact]
    public async Task BuildComplete_Should_Return_Early_When_Source_Branch_Is_Invalid()
    {
        // Arrange
        BuildComplete buildComplete = GetValidBuildComplete();

        buildComplete.SourceBranch = "refs/origin/main";

        // Act
        await _sut.BuildCompleteAsync(buildComplete);

        // Assert
        await _gitProvider
            .Received(0)
            .PostPullRequestStatusAsync(
                Arg.Any<string>(),
                Arg.Any<int>(),
                Arg.Any<PullRequestStatusState>());
    }
    
    [Theory]
    [InlineData(BuildStatus.Failed)]
    [InlineData(BuildStatus.PartiallySucceeded)]
    public async Task BuildComplete_Should_Return_Early_When_Build_Status_Is_Invalid(BuildStatus status)
    {
        // Arrange
        BuildComplete buildComplete = GetValidBuildComplete();
        
        buildComplete.BuildStatus = status;

        // Act
        await _sut.BuildCompleteAsync(buildComplete);

        // Assert
        await _gitProvider
            .Received(0)
            .PostPullRequestStatusAsync(
                Arg.Any<string>(),
                Arg.Any<int>(),
                Arg.Any<PullRequestStatusState>());
    }

    [Fact]
    public async Task BuildComplete_Should_Return_Early_When_Supported_Build_Is_Not_Found()
    {
        // Arrange
        BuildComplete buildComplete = GetValidBuildComplete();

        buildComplete.InternalBuildId = TestInternalBuildId;

        _validator
            .Validate(Arg.Any<ApplicationConfiguration>())
            .Returns(new ValidationResult());

        // Act
        await _sut.BuildCompleteAsync(buildComplete);

        // Assert
        await _gitProvider
            .Received(0)
            .PostPullRequestStatusAsync(
                Arg.Any<string>(),
                Arg.Any<int>(),
                Arg.Any<PullRequestStatusState>());
    }

    [Fact]
    public async Task BuildComplete_Should_Post_Two_Statuses_When_Container_Started_Successfully()
    {
        // Arrange
        BuildComplete buildComplete = GetValidBuildComplete();

        _validator
            .Validate(Arg.Any<ApplicationConfiguration>())
            .Returns(new ValidationResult());

        List<PullRequestStatusState> statusStates = [];

        _gitProvider
            .PostPullRequestStatusAsync(
                TestInternalBuildId,
                buildComplete.PullRequestId,
                Arg.Any<PullRequestStatusState>())
            .Returns(Task.CompletedTask)
            .AndDoes(x => statusStates.Add(x.Arg<PullRequestStatusState>()));

        _gitProvider
            .GetPullRequestById(TestInternalBuildId, buildComplete.PullRequestId)
            .Returns(new PullRequestResponse { Status = "active" });

        _configurationManager
            .GetConfigurationByBuildId(TestInternalBuildId)
            .Returns(GetValidEnvironmentConfiguration());

        // Act
        await _sut.BuildCompleteAsync(buildComplete);

        // Assert
        await _gitProvider
            .Received(2)
            .PostPullRequestStatusAsync(
                TestInternalBuildId,
                buildComplete.PullRequestId,
                Arg.Any<PullRequestStatusState>());

        statusStates.Should().HaveCount(2);

        PullRequestStatusState pendingState = statusStates[0];
        pendingState.Should().Be(PullRequestStatusState.Pending);

        PullRequestStatusState succeededState = statusStates[1];
        succeededState.Should().Be(PullRequestStatusState.Succeeded);
    }

    [Fact]
    public async Task BuildComplete_Should_Use_Port_From_Allowed_Image_Ports()
    {
        // Arrange
        const int expectedPort = 7000;
        BuildComplete buildComplete = GetValidBuildComplete();

        _validator
            .Validate(Arg.Any<ApplicationConfiguration>())
            .Returns(new ValidationResult());

        PreviewEnvironmentConfiguration configuration =
            GetValidEnvironmentConfiguration();

        configuration.Deployment.AllowedDeploymentPorts = [expectedPort];

        _configurationManager
            .GetConfigurationByBuildId(TestInternalBuildId)
            .Returns(configuration);

        int port = 0;
        
        _dockerService
            .RunContainerAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                TestInternalBuildId,
                Arg.Any<int>(),
                Arg.Any<string>(),
                Arg.Any<int>())
            .Returns(new DockerContainer
            {
                ContainerId = string.Empty,
                ImageName = string.Empty,
                ImageTag = string.Empty
            })
            .AndDoes(x => port = x.ArgAt<int>(3));

        _gitProvider
            .GetPullRequestById(TestInternalBuildId, buildComplete.PullRequestId)
            .Returns(new PullRequestResponse { Status = "active" });

        // Act
        await _sut.BuildCompleteAsync(buildComplete);

        // Assert
        port.Should().Be(expectedPort);
    }

    [Fact]
    public async Task BuildComplete_Should_Use_Next_Available_Port_From_Allowed_Image_Ports()
    {
        // Arrange
        const int expectedPort = 7001;
        BuildComplete buildComplete = GetValidBuildComplete();

        _validator
            .Validate(Arg.Any<ApplicationConfiguration>())
            .Returns(new ValidationResult());

        PreviewEnvironmentConfiguration configuration =
            GetValidEnvironmentConfiguration();

        configuration.Deployment.AllowedDeploymentPorts = [7000, expectedPort];
        
        _configurationManager
            .GetConfigurationByBuildId(TestInternalBuildId)
            .Returns(configuration);

        int port = 0;

        _dockerService
            .RunContainerAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                TestInternalBuildId,
                Arg.Any<int>(),
                Arg.Any<string>(),
                Arg.Any<int>())
            .Returns(x => new DockerContainer
            {
                ContainerId = string.Empty,
                ImageName = string.Empty,
                ImageTag = string.Empty,
                Port = x.ArgAt<int>(3),
                InternalBuildId = buildComplete.InternalBuildId,
            })
            .AndDoes(x => port = x.ArgAt<int>(3));

        _gitProvider
            .GetPullRequestById(TestInternalBuildId, buildComplete.PullRequestId)
            .Returns(new PullRequestResponse { Status = "active" });

        await _sut.BuildCompleteAsync(buildComplete);

        // Act
        await _sut.BuildCompleteAsync(buildComplete);

        // Assert
        port.Should().Be(expectedPort);
    }
    
    [Fact]
    public async Task BuildComplete_Should_Post_Failed_Status_When_No_Ports_Available()
    {
        // Arrange
        BuildComplete buildComplete = GetValidBuildComplete();

        _validator
            .Validate(Arg.Any<ApplicationConfiguration>())
            .Returns(new ValidationResult());

        List<PullRequestStatusState> pullRequestStatusStates = [];

        _gitProvider
            .PostPullRequestStatusAsync(
                TestInternalBuildId,
                buildComplete.PullRequestId,
                Arg.Any<PullRequestStatusState>())
            .Returns(Task.CompletedTask)
            .AndDoes(x => pullRequestStatusStates.Add(x.Arg<PullRequestStatusState>()));

        _dockerService
            .RunContainerAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                buildComplete.InternalBuildId,
                Arg.Any<int>(),
                Arg.Any<string>(),
                Arg.Any<int>())
            .Returns(x => new DockerContainer
            {
                ContainerId = string.Empty,
                ImageName = string.Empty,
                ImageTag = string.Empty,
                Port = x.ArgAt<int>(3),
                InternalBuildId = buildComplete.InternalBuildId,
            });

        _gitProvider
            .GetPullRequestById(TestInternalBuildId, buildComplete.PullRequestId)
            .Returns(new PullRequestResponse { Status = "active" });

        _configurationManager
            .GetConfigurationByBuildId(TestInternalBuildId)
            .Returns(GetValidEnvironmentConfiguration());

        // Act
        await _sut.BuildCompleteAsync(buildComplete);
        await _sut.BuildCompleteAsync(buildComplete);

        // Assert
        await _dockerService
            .Received(1)
            .RunContainerAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                buildComplete.InternalBuildId,
                Arg.Any<int>(),
                Arg.Any<string>(),
                Arg.Any<int>());

        pullRequestStatusStates.Should().HaveCount(4);

        PullRequestStatusState failedStatus = pullRequestStatusStates[3];

        failedStatus.Should().Be(PullRequestStatusState.Failed);
    }

    [Fact]
    public async Task BuildComplete_Should_Start_Container_When_Existing_Container_Is_Not_Found()
    {
        // Arrange
        BuildComplete buildComplete = GetValidBuildComplete();

        _validator
            .Validate(Arg.Any<ApplicationConfiguration>())
            .Returns(new ValidationResult());

        _gitProvider
            .GetPullRequestById(TestInternalBuildId, buildComplete.PullRequestId)
            .Returns(new PullRequestResponse { Status = "active" });

        _configurationManager
            .GetConfigurationByBuildId(TestInternalBuildId)
            .Returns(GetValidEnvironmentConfiguration());

        // Act
        await _sut.BuildCompleteAsync(buildComplete);

        // Assert
        await _dockerService
            .Received(1)
            .RunContainerAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                buildComplete.InternalBuildId,
                Arg.Any<int>(),
                Arg.Any<string>(),
                Arg.Any<int>());
    }
    
    [Fact]
    public async Task BuildComplete_Should_Restart_Container_When_Existing_Container_Is_Found()
    {
        // Arrange
        BuildComplete buildComplete = GetValidBuildComplete();

        _validator
            .Validate(Arg.Any<ApplicationConfiguration>())
            .Returns(new ValidationResult());

        _dockerService
            .RunContainerAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                TestInternalBuildId,
                Arg.Any<int>(),
                Arg.Any<string>(),
                Arg.Any<int>())
            .Returns(x => new DockerContainer
            {
                ContainerId = string.Empty,
                ImageName = $"{x.ArgAt<string>(4)}/{x.ArgAt<string>(0)}",
                ImageTag = x.ArgAt<string>(1),
                InternalBuildId = x.ArgAt<string>(2),
                Port = x.ArgAt<int>(3),
            });

        _gitProvider
            .GetPullRequestById(TestInternalBuildId, buildComplete.PullRequestId)
            .Returns(new PullRequestResponse { Status = "active" });

        PreviewEnvironmentConfiguration configuration =
            GetValidEnvironmentConfiguration();

        // HACK: Restarting container fails when exactly 1 port in the allowed
        // ports list.
        configuration.Deployment.AllowedDeploymentPorts = Array.Empty<int>();

        _configurationManager
            .GetConfigurationByBuildId(TestInternalBuildId)
            .Returns(configuration);

        // Act
        await _sut.BuildCompleteAsync(buildComplete);
        await _sut.BuildCompleteAsync(buildComplete);

        // Assert
        await _dockerService
            .Received(1)
            .RestartContainerAsync(
                Arg.Any<DockerContainer>(),
                Arg.Any<int>(),
                cancellationToken: Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task BuildComplete_Should_Use_Deployment_Host_Address_When_Container_Started_Successfully()
    {
        // Arrange
        BuildComplete buildComplete = GetValidBuildComplete();

        _validator
            .Validate(Arg.Any<ApplicationConfiguration>())
            .Returns(new ValidationResult());

        _dockerService
            .RunContainerAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                buildComplete.InternalBuildId,
                Arg.Any<int>(),
                Arg.Any<string>(),
                Arg.Any<int>())
            .Returns(x => new DockerContainer
            {
                ContainerId = string.Empty,
                ImageName = $"{x.ArgAt<string>(4)}/{x.ArgAt<string>(0)}",
                ImageTag = x.ArgAt<string>(1)
            });

        Uri? containerAddress = null;

        _gitProvider
            .PostPreviewAvailableMessageAsync(
                TestInternalBuildId,
                buildComplete.PullRequestId,
                Arg.Any<Uri>())
            .Returns(Task.CompletedTask)
            .AndDoes(x => containerAddress = x.ArgAt<Uri>(2));

        _gitProvider
            .GetPullRequestById(TestInternalBuildId, buildComplete.PullRequestId)
            .Returns(new PullRequestResponse { Status = "active" });

        PreviewEnvironmentConfiguration configuration =
            GetValidEnvironmentConfiguration();

        _configurationManager
            .GetConfigurationByBuildId(TestInternalBuildId)
            .Returns(configuration);

        // Act
        await _sut.BuildCompleteAsync(buildComplete);

        // Assert
        await _gitProvider
            .Received(1)
            .PostPreviewAvailableMessageAsync(
                TestInternalBuildId,
                buildComplete.PullRequestId,
                Arg.Any<Uri>());

        containerAddress.Should().NotBeNull();

        using (new AssertionScope())
        {
            containerAddress!.Scheme.Should().Be(DefaultContainerScheme);
            containerAddress.Host.Should().Be(DefaultContainerHost);
            
            containerAddress.Port
                .Should()
                .Be(configuration.Deployment.AllowedDeploymentPorts.First());
        }
    }
    
    [Fact]
    public async Task BuildComplete_Should_Use_The_Same_Port_When_A_Container_Is_Restarted_Successfully()
    {
        // Arrange
        BuildComplete buildComplete = GetValidBuildComplete();
        
        _validator
            .Validate(Arg.Any<ApplicationConfiguration>())
            .Returns(new ValidationResult());

        int port = 0;

        _dockerService
            .RunContainerAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                buildComplete.InternalBuildId,
                Arg.Any<int>(),
                Arg.Any<string>(),
                Arg.Any<int>())
            .Returns(x => new DockerContainer
            {
                ContainerId = string.Empty,
                ImageName = $"{x.ArgAt<string>(4)}/{x.ArgAt<string>(0)}",
                ImageTag = x.ArgAt<string>(1),
                Port = x.ArgAt<int>(3)
            })
            .AndDoes(x => port = x.ArgAt<int>(3));

        _dockerService
            .RestartContainerAsync(
                Arg.Any<DockerContainer>(),
                Arg.Any<int>())
            .Returns(x => new DockerContainer
            {
                ContainerId = string.Empty,
                ImageName = x.Arg<DockerContainer>().ImageName,
                ImageTag = x.Arg<DockerContainer>().ImageTag
            })
            .AndDoes(x => port = x.Arg<DockerContainer>().Port);

        Uri? deploymentUri = null;

        _gitProvider
            .PostPreviewAvailableMessageAsync(
                TestInternalBuildId,
                buildComplete.PullRequestId,
                Arg.Any<Uri>())
            .Returns(Task.CompletedTask)
            .AndDoes(x => deploymentUri = x.Arg<Uri>());

        _gitProvider
            .GetPullRequestById(TestInternalBuildId, buildComplete.PullRequestId)
            .Returns(new PullRequestResponse { Status = "active" });

        _configurationManager
            .GetConfigurationByBuildId(TestInternalBuildId)
            .Returns(GetValidEnvironmentConfiguration());

        await _sut.BuildCompleteAsync(buildComplete);

        // Act
        await _sut.BuildCompleteAsync(buildComplete);

        // Assert
        await _gitProvider
            .Received(2)
            .PostPreviewAvailableMessageAsync(
                TestInternalBuildId,
                buildComplete.PullRequestId,
                new Uri($"https://test.domain.com:{port}"));

        deploymentUri.Should().NotBeNull();
        deploymentUri.Should().Be(new Uri($"https://test.domain.com:{port}"));
    }

    [Fact]
    public async Task PullRequestUpdated_Should_Return_Early_When_Pull_Request_State_Is_Invalid()
    {
        // Arrange
        PullRequestUpdated pullRequestUpdated = new()
        {
            State = PullRequestState.Active
        };

        // Act
        await _sut.PullRequestUpdatedAsync(pullRequestUpdated);

        // Assert
        await _dockerService
            .Received(0)
            .StopAndRemoveContainerAsync(Arg.Any<string>());
    }
    
    [Fact]
    public async Task PullRequestUpdated_Should_Return_Early_When_Container_Id_Is_Not_Found()
    {
        // Arrange
        PullRequestUpdated pullRequestUpdated = new()
        {
            State = PullRequestState.Completed
        };

        // Act
        await _sut.PullRequestUpdatedAsync(pullRequestUpdated);

        // Assert
        await _dockerService
            .Received(0)
            .StopAndRemoveContainerAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task PullRequestUpdated_Should_Stop_And_Remove_The_Container()
    {
        // Arrange
        const string containerId = "containerId";

        BuildComplete buildComplete = GetValidBuildComplete();

        PullRequestUpdated pullRequestUpdated = new()
        {
            Id = buildComplete.PullRequestId,
            State = PullRequestState.Completed
        };

        _validator
            .Validate(Arg.Any<ApplicationConfiguration>())
            .Returns(new ValidationResult());

        _dockerService
            .RunContainerAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                buildComplete.InternalBuildId,
                Arg.Any<int>(),
                Arg.Any<string>(),
                Arg.Any<int>())
            .Returns(x => new DockerContainer
            {
                ContainerId = containerId,
                ImageName = $"{x.ArgAt<string>(4)}/{x.ArgAt<string>(0)}",
                ImageTag = x.ArgAt<string>(1),
                PullRequestId = buildComplete.PullRequestId
            });

        _gitProvider
            .GetPullRequestById(TestInternalBuildId, buildComplete.PullRequestId)
            .Returns(new PullRequestResponse { Status = "active"});

        _configurationManager
            .GetConfigurationByBuildId(TestInternalBuildId)
            .Returns(GetValidEnvironmentConfiguration());

        await _sut.BuildCompleteAsync(buildComplete);

        // Act
        await _sut.PullRequestUpdatedAsync(pullRequestUpdated);

        // Assert
        await _dockerService
            .Received(1)
            .StopAndRemoveContainerAsync(containerId);
    }

    [Fact]
    public async Task ExpireContainersAsync_Should_Stop_All_Expired_Containers()
    {
        // Arrange
        string[] containerIds =
        [
            "containerId1",
            "containerId2",
            "containerId3",
            "containerId4",
            "containerId5",
        ];

        const int expiredContainerCount = 5;

        int currentIndex = 0;

        BuildComplete buildComplete = GetValidBuildComplete();

        _validator
            .Validate(Arg.Any<ApplicationConfiguration>())
            .Returns(new ValidationResult());

        _dockerService
            .RunContainerAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                buildComplete.InternalBuildId,
                Arg.Any<int>(),
                Arg.Any<string>(),
                Arg.Any<int>())
            .Returns(x => new DockerContainer
            {
                ContainerId = containerIds[currentIndex],
                ImageName = ((char)Random.Shared.Next(65, 91)).ToString(),
                ImageTag = ((char)Random.Shared.Next(65, 91)).ToString(),
                PullRequestId = buildComplete.PullRequestId,
                CreatedTime = DateTime.Now.AddMinutes(-1),
                InternalBuildId = TestInternalBuildId
            });

        _gitProvider
            .GetPullRequestById(TestInternalBuildId, buildComplete.PullRequestId)
            .Returns(new PullRequestResponse { Status = "active" });

        _configurationManager
            .GetConfigurationByBuildId(TestInternalBuildId)
            .Returns(GetValidEnvironmentConfiguration());

        for (int i = 0; i < expiredContainerCount; i++)
        {
            currentIndex = i;
            await _sut.BuildCompleteAsync(buildComplete);
        }

        // Act
        await _sut.ExpireContainersAsync();

        // Assert
        await _dockerService
            .Received(expiredContainerCount)
            .StopContainerAsync(Arg.Is<string>(s => containerIds.Contains(s)));

        await _gitProvider
            .Received(expiredContainerCount)
            .PostExpiredContainerMessageAsync(
                TestInternalBuildId,
                buildComplete.PullRequestId);
    }

    [Fact]
    public async Task DisposeAsync_Should_Stop_And_Remove_All_Containers()
    {
        // Arrange
        string[] containerIds =
        [
            "containerId1",
            "containerId2",
            "containerId3",
            "containerId4",
            "containerId5",
        ];

        const int containerCount = 5;

        int currentIndex = 0;

        BuildComplete buildComplete = GetValidBuildComplete();

        _validator
            .Validate(Arg.Any<ApplicationConfiguration>())
            .Returns(new ValidationResult());

        _dockerService
            .RunContainerAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                buildComplete.InternalBuildId,
                Arg.Any<int>(),
                Arg.Any<string>(),
                Arg.Any<int>())
            .Returns(x => new DockerContainer
            {
                ContainerId = containerIds[currentIndex],
                ImageName = ((char)Random.Shared.Next(65, 91)).ToString(),
                ImageTag = ((char)Random.Shared.Next(65, 91)).ToString(),
                PullRequestId = buildComplete.PullRequestId
            });

        _gitProvider
            .GetPullRequestById(TestInternalBuildId, buildComplete.PullRequestId)
            .Returns(new PullRequestResponse { Status = "active" });

        _configurationManager
            .GetConfigurationByBuildId(TestInternalBuildId)
            .Returns(GetValidEnvironmentConfiguration());

        for (int i = 0; i < containerCount; i++)
        {
            currentIndex = i;
            await _sut.BuildCompleteAsync(buildComplete);
        }

        // Act
        await _sut.DisposeAsync();

        // Assert
        await _dockerService
            .Received(containerCount)
            .StopAndRemoveContainerAsync(Arg.Is<string>(s => containerIds.Contains(s)));

        _dockerService
            .Received(1)
            .Dispose();
    }

    private static BuildComplete GetValidBuildComplete()
    {
        return new BuildComplete
        {
            SourceBranch = "refs/pull/1",
            InternalBuildId = TestInternalBuildId,
            BuildStatus = BuildStatus.Succeeded,
            BuildUrl = new Uri("https://dev.azure.com"),
            PullRequestId = 1
        };
    }

    private static PreviewEnvironmentConfiguration GetValidEnvironmentConfiguration()
    {
        return new PreviewEnvironmentConfiguration
        {
            GitProvider = Constants.GitProviders.AzureRepos,
            Deployment = new Deployment
            {
                ContainerHostAddress =
                    $"{DefaultContainerScheme}://{DefaultContainerHost}",
                AllowedDeploymentPorts = [24302]
            }
        };
    }
}