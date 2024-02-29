using Microsoft.Extensions.Logging;
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
    
    private readonly ICommandHandler _sut;

    public CommandHandlerTests()
    {
        _dockerService = Substitute.For<IDockerService>();
        _containerTracker = Substitute.For<IContainerTracker>();
        _gitProviderFactory = Substitute.For<IGitProviderFactory>();
        _configurationManager = Substitute.For<IConfigurationManager>();
        
        _sut = new CommandHandler(
            Substitute.For<ILogger<CommandHandler>>(),
            _dockerService,
            _containerTracker,
            _gitProviderFactory,
            _configurationManager);
    }

    [Fact]
    public async Task HandleAsync_Restart_Command_Should_Restart_Container()
    {
        // Arrange
        const string internalBuildId = "test-internal-build-id";
        
        DockerContainer existingContainer = new()
        {
            ContainerId = string.Empty,
            ImageName = string.Empty,
            ImageTag = string.Empty,
            InternalBuildId = internalBuildId,
        };

        DockerContainer newContainer = new()
        {
            ContainerId = string.Empty,
            ImageName = string.Empty,
            ImageTag = string.Empty,
            InternalBuildId = internalBuildId,
        };
        
        _containerTracker
            .SingleOrDefault(Arg.Any<Predicate<DockerContainer>>())
            .Returns(existingContainer);

        _dockerService
            .RestartContainerAsync(Arg.Any<DockerContainer>())
            .Returns(newContainer);

        // Act
        await _sut.HandleAsync("/pe restart", new CommandMetadata
        {
            GitProvider = Constants.GitProviders.AzureRepos,
        });

        // Assert
        await _dockerService
            .Received(1)
            .RestartContainerAsync(existingContainer);

        _containerTracker.Received(1).Remove(string.Empty);
        _containerTracker.Received(1).Add(string.Empty, newContainer);

        _configurationManager
            .Received(1)
            .GetConfigurationByBuildId(internalBuildId);
    }

    [Fact]
    public async Task HandleAsync_Missing_Command_Identifier_Should_Return_Early()
    {
        // Act
        await _sut.HandleAsync("hello world", new CommandMetadata());

        // Assert
        _dockerService.ReceivedCalls().Should().BeEmpty();
        _containerTracker.ReceivedCalls().Should().BeEmpty();
        _configurationManager.ReceivedCalls().Should().BeEmpty();
        _gitProviderFactory.ReceivedCalls().Should().BeEmpty();
    }
}