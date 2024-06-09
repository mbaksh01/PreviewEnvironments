using Microsoft.Extensions.Logging;
using PreviewEnvironments.Application.Models;
using PreviewEnvironments.Application.Models.Commands;
using PreviewEnvironments.Application.Models.Docker;
using PreviewEnvironments.Application.Services;
using PreviewEnvironments.Application.Services.Abstractions;

namespace PreviewEnvironments.Application.Test.Unit.Services;

public class CommandHandlerTests
{
    private readonly IDockerService _dockerService;
    private readonly IContainerTracker _containerTracker;
    private readonly IGitProviderFactory _gitProviderFactory;
    private readonly IConfigurationManager _configurationManager;
    private readonly IRedirectService _redirectService;
    
    private readonly ICommandHandler _sut;

    public CommandHandlerTests()
    {
        _dockerService = Substitute.For<IDockerService>();
        _containerTracker = Substitute.For<IContainerTracker>();
        _gitProviderFactory = Substitute.For<IGitProviderFactory>();
        _configurationManager = Substitute.For<IConfigurationManager>();
        _redirectService = Substitute.For<IRedirectService>();
        
        _sut = new CommandHandler(
            Substitute.For<ILogger<CommandHandler>>(),
            _dockerService,
            _containerTracker,
            _gitProviderFactory,
            _configurationManager,
            _redirectService);
    }

    [Fact]
    public async Task HandleAsync_Restart_Command_Should_Restart_Container()
    {
        // Arrange
        const string internalBuildId = "test-internal-build-id";
        const string containerHostAddress = "https://test.application.com";
        const int pullRequestId = 1;
        string containerId = new('A', 12);
        Uri host = new("https://test.host.com");
        IGitProvider gitProvider = Substitute.For<IGitProvider>();
        
        DockerContainer existingContainer = new()
        {
            ContainerId = containerId,
            ImageName = string.Empty,
            ImageTag = string.Empty,
            InternalBuildId = internalBuildId,
        };

        DockerContainer newContainer = new()
        {
            ContainerId = containerId,
            ImageName = string.Empty,
            ImageTag = string.Empty,
            InternalBuildId = internalBuildId,
            Port = 80
        };
        
        _containerTracker
            .SingleOrDefault(Arg.Any<Predicate<DockerContainer>>())
            .Returns(existingContainer);

        _gitProviderFactory
            .CreateProvider(GitProvider.AzureRepos)
            .Returns(gitProvider);

        _dockerService
            .RestartContainerAsync(Arg.Any<DockerContainer>())
            .Returns(newContainer);

        _configurationManager
            .GetConfigurationById(internalBuildId)
            .Returns(new PreviewEnvironmentConfiguration
            {
                Deployment = new Deployment
                {
                    ContainerHostAddress = containerHostAddress,
                }
            });

        // Act
        await _sut.HandleAsync("/pe restart", new CommandMetadata
        {
            GitProvider = Constants.GitProviders.AzureRepos,
            Host = host,
            PullRequestId = pullRequestId
        });

        // Assert
        await _dockerService
            .Received(1)
            .RestartContainerAsync(existingContainer);

        _containerTracker.Received(1).Remove(containerId);
        _containerTracker.Received(1).Add(containerId, newContainer);

        _configurationManager
            .Received(1)
            .GetConfigurationById(internalBuildId);

        _redirectService
            .Received(1)
            .Add(
                containerId,
                Arg.Is<Uri>(u => u.AbsoluteUri == "https://test.application.com:80/"),
                host);
        
        await gitProvider
            .Received(1)
            .PostPullRequestMessageAsync(
                newContainer.InternalBuildId,
                pullRequestId,
                Arg.Any<string>());
    }

    [Fact]
    public async Task HandleAsync_Should_Return_Early_When_Command_Identifier_Is_Missing()
    {
        // Act
        await _sut.HandleAsync("hello world", new CommandMetadata());

        // Assert
        _dockerService.ReceivedCalls().Should().BeEmpty();
        _containerTracker.ReceivedCalls().Should().BeEmpty();
        _configurationManager.ReceivedCalls().Should().BeEmpty();
        _gitProviderFactory.ReceivedCalls().Should().BeEmpty();
    }
    
    [Fact]
    public async Task HandleAsync_Should_Return_Early_When_Docker_Container_Is_Not_Found()
    {
        // Arrange
        const GitProvider gitProviderName = GitProvider.AzureRepos;
        const int pullRequestId = 1;
        IGitProvider gitProvider = Substitute.For<IGitProvider>();
        
        _gitProviderFactory
            .CreateProvider(gitProviderName)
            .Returns(gitProvider);
        
        // Act
        await _sut.HandleAsync("/pe restart", new CommandMetadata
        {
            GitProvider = Constants.GitProviders.AzureRepos,
            PullRequestId = pullRequestId
        });

        // Assert
        _dockerService.ReceivedCalls().Should().BeEmpty();
        _configurationManager.ReceivedCalls().Should().BeEmpty();
        
        _containerTracker.Received(1).SingleOrDefault(Arg.Any<Predicate<DockerContainer>>());
        
        _gitProviderFactory
            .Received(1)
            .CreateProvider(gitProviderName);
        
        await gitProvider
            .Received(1)
            .PostPullRequestMessageAsync(
                Arg.Any<string>(),
                pullRequestId,
                Arg.Any<string>());
    }
    
    [Fact]
    public async Task HandleAsync_Should_Return_Early_When_Restart_Failed()
    {
        // Arrange
        const GitProvider gitProviderName = GitProvider.AzureRepos;
        const int pullRequestId = 1;
        IGitProvider gitProvider = Substitute.For<IGitProvider>();

        _containerTracker
            .SingleOrDefault(Arg.Any<Predicate<DockerContainer>>())
            .Returns(new DockerContainer
            {
                ContainerId = string.Empty,
                ImageName = string.Empty,
                ImageTag = string.Empty
            });
        
        _gitProviderFactory
            .CreateProvider(gitProviderName)
            .Returns(gitProvider);
        
        // Act
        await _sut.HandleAsync("/pe restart", new CommandMetadata
        {
            GitProvider = Constants.GitProviders.AzureRepos,
            PullRequestId = pullRequestId
        });

        // Assert
        _configurationManager.ReceivedCalls().Should().BeEmpty();
        gitProvider.ReceivedCalls().Should().BeEmpty();
        
        await _dockerService
            .Received(1)
            .RestartContainerAsync(Arg.Any<DockerContainer>());
        
        _containerTracker.Received(1).SingleOrDefault(Arg.Any<Predicate<DockerContainer>>());
        
        _gitProviderFactory
            .Received(1)
            .CreateProvider(gitProviderName);
    }
    
    [Fact]
    public async Task HandleAsync_Should_Return_Early_When_Configuration_Is_Not_Found()
    {
        // Arrange
        const string testInternalBuildId = "test-internal-build-id";
        const int pullRequestId = 1;

        _containerTracker
            .SingleOrDefault(Arg.Any<Predicate<DockerContainer>>())
            .Returns(new DockerContainer
            {
                ContainerId = string.Empty,
                ImageName = string.Empty,
                ImageTag = string.Empty
            });

        _dockerService
            .RestartContainerAsync(Arg.Any<DockerContainer>())
            .Returns(new DockerContainer
            {
                ContainerId = string.Empty,
                ImageName = string.Empty,
                ImageTag = string.Empty,
                InternalBuildId = testInternalBuildId
            });
        
        // Act
        await _sut.HandleAsync("/pe restart", new CommandMetadata
        {
            GitProvider = Constants.GitProviders.AzureRepos,
            PullRequestId = pullRequestId
        });

        // Assert
        _containerTracker.Received(1).SingleOrDefault(Arg.Any<Predicate<DockerContainer>>());
        
        await _dockerService
            .Received(1)
            .RestartContainerAsync(Arg.Any<DockerContainer>());
        
        _configurationManager.Received(1).GetConfigurationById(testInternalBuildId);

        _redirectService.ReceivedCalls().Should().BeEmpty();
    }
}