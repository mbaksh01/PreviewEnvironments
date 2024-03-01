using Docker.DotNet.Models;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PreviewEnvironments.Application.Extensions;
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
    private readonly IContainerTracker _containers;

    public PreviewEnvironmentManagerTests()
    {
        _gitProviderFactory = Substitute.For<IGitProviderFactory>();
        _gitProvider = Substitute.For<IGitProvider>();
        _validator = Substitute.For<IValidator<ApplicationConfiguration>>();
        _options = Options.Create(new ApplicationConfiguration());
        _dockerService = Substitute.For<IDockerService>();
        _configurationManager = Substitute.For<IConfigurationManager>();
        _containers = Substitute.For<IContainerTracker>();

        _sut = new PreviewEnvironmentManager(
            Substitute.For<ILogger<PreviewEnvironmentManager>>(),
            _validator,
            _options,
            _gitProviderFactory,
            _dockerService,
            _configurationManager,
            _containers);

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

        PullRequestUpdated pullRequestUpdated = new()
        {
            Id = 1,
            State = PullRequestState.Completed
        };

        _containers
            .SingleOrDefault(Arg.Any<Predicate<DockerContainer>>())
            .Returns(new DockerContainer
            {
                ContainerId = containerId,
                ImageName = string.Empty,
                ImageTag = string.Empty
            });

        // Act
        await _sut.PullRequestUpdatedAsync(pullRequestUpdated);

        // Assert
        await _dockerService
            .Received(1)
            .StopAndRemoveContainerAsync(containerId);
    }

    [Fact]
    public async Task ExpireContainersAsync_Should_Stop_All_Containers_Which_Have_Timed_Out()
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

        const int expiredContainerCount = 4;

        BuildComplete buildComplete = GetValidBuildComplete();

        _configurationManager
            .GetConfigurationById(TestInternalBuildId)
            .Returns(GetValidEnvironmentConfiguration());

        _containers
            .Where(Arg.Any<Predicate<DockerContainer>>())
            .Returns(Enumerable.Range(0, containerIds.Length).Select(i =>
                new DockerContainer
                {
                    ContainerId = $"containerId{i + 1}",
                    ImageName = string.Empty,
                    ImageTag = string.Empty,
                    InternalBuildId = TestInternalBuildId,
                    CreatedTime = DateTime.Now.AddMinutes(-i),
                    PullRequestId = buildComplete.PullRequestId,
                }));

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
    public async Task ExpireContainersAsync_Should_Stop_Only_Running_Containers()
    {
        // Arrange
        Predicate<DockerContainer>? predicate = null;
        
        _containers
            .Where(Arg.Any<Predicate<DockerContainer>>())
            .Returns(Enumerable.Empty<DockerContainer>())
            .AndDoes(x => predicate = x.Arg<Predicate<DockerContainer>>());

        IEnumerable<DockerContainer> containers = Enumerable.Range(0, 6).Select(i =>
            new DockerContainer
            {
                ContainerId = string.Empty,
                ImageName = string.Empty,
                ImageTag = string.Empty,
                Expired = i % 2 == 1,
            });

        // Act
        await _sut.ExpireContainersAsync();

        // Assert
        predicate.Should().NotBeNull();
        containers.Where(predicate!.Invoke).Should().HaveCount(3);
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

        _containers
            .GetKeys()
            .Returns(containerIds);

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
                AllowedDeploymentPorts = [24302],
                ContainerTimeoutSeconds = 60
            }
        };
    }
}