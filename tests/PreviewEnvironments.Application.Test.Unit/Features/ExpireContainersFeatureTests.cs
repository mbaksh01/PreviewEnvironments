using Microsoft.Extensions.Logging;
using PreviewEnvironments.Application.Features;
using PreviewEnvironments.Application.Features.Abstractions;
using PreviewEnvironments.Application.Models;
using PreviewEnvironments.Application.Models.AzureDevOps.Builds;
using PreviewEnvironments.Application.Models.Docker;
using PreviewEnvironments.Application.Services;
using PreviewEnvironments.Application.Services.Abstractions;

namespace PreviewEnvironments.Application.Test.Unit.Features;

public class ExpireContainersFeatureTests
{
    private const string TestInternalBuildId = "test-internal-build-id";
    private const string DefaultContainerScheme = "https";
    private const string DefaultContainerHost = "test.domain.com";

    private readonly IExpireContainersFeature _sut;
    private readonly IDockerService _dockerService;
    private readonly IConfigurationManager _configurationManager;
    private readonly IContainerTracker _containers;
    private readonly IGitProvider _gitProvider;

    public ExpireContainersFeatureTests()
    {
        _dockerService = Substitute.For<IDockerService>();
        _configurationManager = Substitute.For<IConfigurationManager>();
        _containers = Substitute.For<IContainerTracker>();
        _gitProvider = Substitute.For<IGitProvider>();

        IGitProviderFactory factory = Substitute.For<IGitProviderFactory>();

        factory
            .CreateProvider(Arg.Any<GitProvider>())
            .Returns(_gitProvider);

        _sut = new ExpireContainersFeature(
            Substitute.For<ILogger<ExpireContainersFeature>>(),
            _dockerService,
            _containers,
            _configurationManager,
            factory);
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
            .PostPullRequestMessageAsync(
                TestInternalBuildId,
                buildComplete.PullRequestId,
                Arg.Any<string>());
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