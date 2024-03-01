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