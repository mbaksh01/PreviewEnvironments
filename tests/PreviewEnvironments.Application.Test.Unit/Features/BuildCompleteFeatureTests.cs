using Microsoft.Extensions.Logging;
using PreviewEnvironments.Application.Features;
using PreviewEnvironments.Application.Features.Abstractions;
using PreviewEnvironments.Application.Models;
using PreviewEnvironments.Application.Models.AzureDevOps.Builds;
using PreviewEnvironments.Application.Models.AzureDevOps.Contracts;
using PreviewEnvironments.Application.Models.AzureDevOps.PullRequests;
using PreviewEnvironments.Application.Models.Docker;
using PreviewEnvironments.Application.Services;
using PreviewEnvironments.Application.Services.Abstractions;

namespace PreviewEnvironments.Application.Test.Unit.Features;

public class BuildCompleteFeatureTests
{
    private const string TestInternalBuildId = "test-internal-build-id";
    private const string DefaultContainerScheme = "https";
    private const string DefaultContainerHost = "test.domain.com";

    private readonly IBuildCompleteFeature _sut;
    private readonly IGitProvider _gitProvider;
    private readonly IDockerService _dockerService;
    private readonly IConfigurationManager _configurationManager;
    private readonly IContainerTracker _containers;
    private readonly IRedirectService _redirectService;

    public BuildCompleteFeatureTests()
    {
        _gitProvider = Substitute.For<IGitProvider>();
        _dockerService = Substitute.For<IDockerService>();
        _configurationManager = Substitute.For<IConfigurationManager>();
        _containers = Substitute.For<IContainerTracker>();
        _redirectService = Substitute.For<IRedirectService>();

        IGitProviderFactory factory = Substitute.For<IGitProviderFactory>();

        factory
            .CreateProvider(Arg.Any<GitProvider>())
            .Returns(_gitProvider);
        
        _sut = new BuildCompleteFeature(
            Substitute.For<ILogger<BuildCompleteFeature>>(),
            factory,
            _dockerService,
            _containers,
            _configurationManager,
            _redirectService);
    }

    [Fact]
    public async Task BuildComplete_Should_Return_Early_When_Source_Branch_Is_Invalid()
    {
        // Arrange
        BuildComplete buildComplete = GetValidBuildComplete();

        buildComplete.SourceBranch = "refs/origin/main";

        // Act
        string? id = await _sut.BuildCompleteAsync(buildComplete);

        // Assert
        id.Should().BeNull();
        
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
        string? id = await _sut.BuildCompleteAsync(buildComplete);

        // Assert
        id.Should().BeNull();
        
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

        // Act
        string? id = await _sut.BuildCompleteAsync(buildComplete);

        // Assert
        id.Should().BeNull();
        
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

        PreviewEnvironmentConfiguration configuration =
            GetValidEnvironmentConfiguration();
        
        _configurationManager
            .GetConfigurationById(TestInternalBuildId)
            .Returns(configuration);

        _dockerService
            .RunContainerAsync(
                configuration.Deployment.ImageName,
                $"pr-{buildComplete.PullRequestId}",
                TestInternalBuildId,
                configuration.Deployment.AllowedDeploymentPorts[0],
                configuration.Deployment.ImageRegistry,
                startContainer: !configuration.Deployment.ColdStartEnabled)
            .Returns(new DockerContainer
            {
                ContainerId = new string('A', 12),
                ImageName = string.Empty,
                ImageTag = string.Empty,
            });

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

        PreviewEnvironmentConfiguration configuration =
            GetValidEnvironmentConfiguration();

        configuration.Deployment.AllowedDeploymentPorts = [expectedPort];

        _configurationManager
            .GetConfigurationById(TestInternalBuildId)
            .Returns(configuration);

        int port = 0;
        
        _dockerService
            .RunContainerAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                TestInternalBuildId,
                Arg.Any<int>(),
                Arg.Any<string>(),
                Arg.Any<int>(),
                startContainer: Arg.Any<bool>())
            .Returns(new DockerContainer
            {
                ContainerId = new string('A', 12),
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

        PreviewEnvironmentConfiguration configuration =
            GetValidEnvironmentConfiguration();

        configuration.Deployment.AllowedDeploymentPorts = [7000, expectedPort];
        
        _configurationManager
            .GetConfigurationById(TestInternalBuildId)
            .Returns(configuration);

        int port = 0;

        _dockerService
            .RunContainerAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                TestInternalBuildId,
                Arg.Any<int>(),
                Arg.Any<string>(),
                Arg.Any<int>(),
                startContainer: Arg.Any<bool>())
            .Returns(x => new DockerContainer
            {
                ContainerId = new string('A', 12),
                ImageName = string.Empty,
                ImageTag = string.Empty,
                Port = x.ArgAt<int>(3),
                InternalBuildId = buildComplete.InternalBuildId,
            })
            .AndDoes(x => port = x.ArgAt<int>(3));

        _gitProvider
            .GetPullRequestById(TestInternalBuildId, buildComplete.PullRequestId)
            .Returns(new PullRequestResponse { Status = "active" });

        _containers
            .Where(Arg.Any<Predicate<DockerContainer>>())
            .Returns([
                new DockerContainer
                {
                    ContainerId = string.Empty,
                    ImageName = string.Empty,
                    ImageTag = string.Empty,
                    Port = configuration.Deployment.AllowedDeploymentPorts[0],
                }
            ]);

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

        PreviewEnvironmentConfiguration configuration =
            GetValidEnvironmentConfiguration();
        
        _configurationManager
            .GetConfigurationById(TestInternalBuildId)
            .Returns(configuration);

        _containers
            .Where(Arg.Any<Predicate<DockerContainer>>())
            .Returns([
                new DockerContainer
                {
                    ContainerId = "",
                    ImageName = "",
                    ImageTag = "",
                    Port = configuration.Deployment.AllowedDeploymentPorts[0]
                }
            ]);

        // Act
        string? id = await _sut.BuildCompleteAsync(buildComplete);

        // Assert
        id.Should().BeNull();
        
        pullRequestStatusStates.Should().HaveCount(2);

        PullRequestStatusState failedStatus = pullRequestStatusStates[1];

        failedStatus.Should().Be(PullRequestStatusState.Failed);
    }
    
    [Fact]
    public async Task BuildComplete_Should_Post_Failed_Status_When_Docker_Returns_Null()
    {
        // Arrange
        BuildComplete buildComplete = GetValidBuildComplete();

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
            .Returns((DockerContainer?)null);

        _gitProvider
            .GetPullRequestById(TestInternalBuildId, buildComplete.PullRequestId)
            .Returns(new PullRequestResponse { Status = "active" });

        PreviewEnvironmentConfiguration configuration =
            GetValidEnvironmentConfiguration();
        
        _configurationManager
            .GetConfigurationById(TestInternalBuildId)
            .Returns(configuration);

        _containers
            .Where(Arg.Any<Predicate<DockerContainer>>())
            .Returns([
                new DockerContainer
                {
                    ContainerId = "",
                    ImageName = "",
                    ImageTag = "",
                    Port = configuration.Deployment.AllowedDeploymentPorts[0]
                }
            ]);

        // Act
        string? id = await _sut.BuildCompleteAsync(buildComplete);

        // Assert
        id.Should().BeNull();
        
        pullRequestStatusStates.Should().HaveCount(2);

        PullRequestStatusState failedStatus = pullRequestStatusStates[1];

        failedStatus.Should().Be(PullRequestStatusState.Failed);
    }

    [Fact]
    public async Task BuildComplete_Should_Start_Container_When_Existing_Container_Is_Not_Found()
    {
        // Arrange
        BuildComplete buildComplete = GetValidBuildComplete();

        _gitProvider
            .GetPullRequestById(TestInternalBuildId, buildComplete.PullRequestId)
            .Returns(new PullRequestResponse { Status = "active" });

        _configurationManager
            .GetConfigurationById(TestInternalBuildId)
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
                Arg.Any<int>(),
                startContainer: Arg.Any<bool>());
    }

    [Fact]
    public async Task BuildComplete_Should_Restart_Container_When_Existing_Container_Is_Found()
    {
        // Arrange
        BuildComplete buildComplete = GetValidBuildComplete();

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

        // HACK: Restarting container fails when exactly 1 port is in the
        // allowed ports list.
        // configuration.Deployment.AllowedDeploymentPorts = Array.Empty<int>();

        _configurationManager
            .GetConfigurationById(TestInternalBuildId)
            .Returns(configuration);

        _containers
            .SingleOrDefault(Arg.Any<Predicate<DockerContainer>>())
            .Returns(new DockerContainer
            {
                ContainerId = string.Empty,
                ImageName = string.Empty,
                ImageTag = string.Empty,
            });

        // Act
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
    public async Task BuildComplete_Should_Use_Redirect_Address_When_Container_Started_Successfully()
    {
        // Arrange
        BuildComplete buildComplete = GetValidBuildComplete();

        int port = 0;
        
        _dockerService
            .RunContainerAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                buildComplete.InternalBuildId,
                Arg.Any<int>(),
                Arg.Any<string>(),
                Arg.Any<int>(),
                startContainer: Arg.Any<bool>())
            .Returns(x => new DockerContainer
            {
                ContainerId = new string('A', 12),
                ImageName = $"{x.ArgAt<string>(4)}/{x.ArgAt<string>(0)}",
                ImageTag = x.ArgAt<string>(1)
            })
            .AndDoes(x => port = x.ArgAt<int>(3));

        _gitProvider
            .GetPullRequestById(TestInternalBuildId, buildComplete.PullRequestId)
            .Returns(new PullRequestResponse { Status = "active" });

        PreviewEnvironmentConfiguration configuration =
            GetValidEnvironmentConfiguration();

        _configurationManager
            .GetConfigurationById(TestInternalBuildId)
            .Returns(configuration);

        _redirectService
            .Add(Arg.Any<string>(), Arg.Any<Uri>(), Arg.Any<Uri>())
            .Returns(new Uri("https://test.application.com"));

        // Act
        await _sut.BuildCompleteAsync(buildComplete);

        // Assert
        await _gitProvider
            .Received(1)
            .PostPullRequestMessageAsync(
                TestInternalBuildId,
                buildComplete.PullRequestId,
                Arg.Is<string>(s => s.Contains("https://test.application.com")));
    }
    
    [Fact]
    public async Task BuildComplete_Should_Use_The_Same_Port_When_A_Container_Is_Restarted_Successfully()
    {
        // Arrange
        BuildComplete buildComplete = GetValidBuildComplete();

        int initialPort = Random.Shared.Next(10_000, 60_000);
        int restartPort = 0;

        _dockerService
            .RestartContainerAsync(
                Arg.Any<DockerContainer>(),
                Arg.Any<int>())
            .Returns(x => new DockerContainer
            {
                ContainerId = x.Arg<DockerContainer>().ContainerId,
                ImageName = x.Arg<DockerContainer>().ImageName,
                ImageTag = x.Arg<DockerContainer>().ImageTag
            })
            .AndDoes(x => restartPort = x.Arg<DockerContainer>().Port);

        _gitProvider
            .GetPullRequestById(TestInternalBuildId, buildComplete.PullRequestId)
            .Returns(new PullRequestResponse { Status = "active" });

        PreviewEnvironmentConfiguration configuration =
            GetValidEnvironmentConfiguration();
        
        _configurationManager
            .GetConfigurationById(TestInternalBuildId)
            .Returns(configuration);

        _containers
            .SingleOrDefault(Arg.Any<Predicate<DockerContainer>>())
            .Returns(new DockerContainer
            {
                ContainerId = new string('A', 12),
                ImageName = string.Empty,
                ImageTag = string.Empty,
                Port = initialPort,
            });
        
        _redirectService
            .Add(Arg.Any<string>(), Arg.Any<Uri>(), Arg.Any<Uri>())
            .Returns(new Uri("https://test.application.com"));

        // Act
        await _sut.BuildCompleteAsync(buildComplete);

        // Assert
        await _gitProvider
            .Received(1)
            .PostPullRequestMessageAsync(
                TestInternalBuildId,
                buildComplete.PullRequestId,
                Arg.Is<string>(s => s.Contains("https://test.application.com")));

        restartPort.Should().Be(initialPort);
    }
    
    [Fact]
    public async Task BuildComplete_Should_Return_12_Character_Id()
    {
        // Arrange
        BuildComplete buildComplete = GetValidBuildComplete();
        
        _dockerService
            .RestartContainerAsync(
                Arg.Any<DockerContainer>(),
                Arg.Any<int>())
            .Returns(x => new DockerContainer
            {
                ContainerId = x.Arg<DockerContainer>().ContainerId,
                ImageName = x.Arg<DockerContainer>().ImageName,
                ImageTag = x.Arg<DockerContainer>().ImageTag
            });

        _gitProvider
            .GetPullRequestById(TestInternalBuildId, buildComplete.PullRequestId)
            .Returns(new PullRequestResponse { Status = "active" });
        
        _configurationManager
            .GetConfigurationById(TestInternalBuildId)
            .Returns(GetValidEnvironmentConfiguration());

        _containers
            .SingleOrDefault(Arg.Any<Predicate<DockerContainer>>())
            .Returns(new DockerContainer
            {
                ContainerId = new string('A', 24),
                ImageName = string.Empty,
                ImageTag = string.Empty
            });

        // Act
        string? id = await _sut.BuildCompleteAsync(buildComplete);

        // Assert
        id.Should().Be(new string('A', 12));
    }
    
    [Fact]
    public async Task BuildComplete_Should_Add_Container_To_Container_Tracker()
    {
        // Arrange
        BuildComplete buildComplete = GetValidBuildComplete();
        string containerId = new('A', 24);
        
        _dockerService
            .RestartContainerAsync(
                Arg.Any<DockerContainer>(),
                Arg.Any<int>())
            .Returns(x => new DockerContainer
            {
                ContainerId = x.Arg<DockerContainer>().ContainerId,
                ImageName = x.Arg<DockerContainer>().ImageName,
                ImageTag = x.Arg<DockerContainer>().ImageTag
            });

        _gitProvider
            .GetPullRequestById(TestInternalBuildId, buildComplete.PullRequestId)
            .Returns(new PullRequestResponse { Status = "active" });
        
        _configurationManager
            .GetConfigurationById(TestInternalBuildId)
            .Returns(GetValidEnvironmentConfiguration());

        _containers
            .SingleOrDefault(Arg.Any<Predicate<DockerContainer>>())
            .Returns(new DockerContainer
            {
                ContainerId = containerId,
                ImageName = string.Empty,
                ImageTag = string.Empty
            });

        // Act
        await _sut.BuildCompleteAsync(buildComplete);

        // Assert
        _containers
            .Received(1)
            .Add(
                containerId,
                Arg.Is<DockerContainer>(c => c.ContainerId == containerId));
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
                AllowedDeploymentPorts = [24302],
                ContainerTimeoutSeconds = 60,
                ImageName = "test-image-name",
                ImageRegistry = "test.registry.com",
                ColdStartEnabled = true
            }
        };
    }
}