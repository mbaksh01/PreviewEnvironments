using CommandLine;
using Microsoft.Extensions.Logging;
using PreviewEnvironments.Application.Extensions;
using PreviewEnvironments.Application.Models;
using PreviewEnvironments.Application.Models.Commands;
using PreviewEnvironments.Application.Models.Docker;
using PreviewEnvironments.Application.Services.Abstractions;

namespace PreviewEnvironments.Application.Services;

internal sealed partial class CommandHandler : ICommandHandler
{
    private readonly ILogger<CommandHandler> _logger;
    private readonly IDockerService _dockerService;
    private readonly IContainerTracker _containerTracker;
    private readonly IGitProviderFactory _gitProviderFactory;
    private readonly IConfigurationManager _configurationManager;

    public CommandHandler(
        ILogger<CommandHandler> logger,
        IDockerService dockerService,
        IContainerTracker containerTracker,
        IGitProviderFactory gitProviderFactory,
        IConfigurationManager configurationManager)
    {
        _logger = logger;
        _dockerService = dockerService;
        _containerTracker = containerTracker;
        _gitProviderFactory = gitProviderFactory;
        _configurationManager = configurationManager;
    }

    public async Task HandleAsync(string comment, CommandMetadata metadata, CancellationToken cancellationToken = default)
    {
        if (comment.StartsWith("/pe") == false)
        {
            return;
        }

        string[] args = comment.Split(' ')[1..];
        
        ParserResult<RestartCommand>? command =
            Parser.Default.ParseArguments<RestartCommand>(args);

        await command.MapResult(
            restartCommand => RestartAsync(metadata, cancellationToken),
            _ => Task.CompletedTask);
    }

    private async Task RestartAsync(CommandMetadata metadata, CancellationToken cancellationToken)
    {
        DockerContainer? existingContainer = _containerTracker.SingleOrDefault(c =>
            c.PullRequestId == metadata.PullRequestId);

        if (existingContainer is null)
        {
            Log.ContainerNotFound(_logger, metadata.PullRequestId);
            return;
        }

        DockerContainer? newContainer =
            await _dockerService.RestartContainerAsync(
                existingContainer,
                cancellationToken: cancellationToken);

        if (newContainer is null)
        {
            Log.FailedToStartContainer(_logger, metadata.PullRequestId);
            return;
        }

        _containerTracker.Remove(existingContainer.ContainerId);
        _containerTracker.Add(newContainer.ContainerId, newContainer);

        GitProvider gitProviderType =
            metadata.GitProvider.GetGitProviderFromString();

        IGitProvider gitProvider =
            _gitProviderFactory.CreateProvider(gitProviderType);

        PreviewEnvironmentConfiguration? configuration = _configurationManager
            .GetConfigurationByBuildId(newContainer.InternalBuildId);

        if (configuration is null)
        {
            Log.ConfigurationNotFound(_logger, newContainer.InternalBuildId);
            return;
        }
        
        Uri address =
            new($"{configuration.Deployment.ContainerHostAddress}:{newContainer.Port}");
        
        await gitProvider.PostPreviewAvailableMessageAsync(
            newContainer.InternalBuildId,
            metadata.PullRequestId,
            address,
            cancellationToken);
    }
}