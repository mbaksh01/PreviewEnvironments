﻿using CommandLine;
using Microsoft.Extensions.Logging;
using PreviewEnvironments.Application.Extensions;
using PreviewEnvironments.Application.Helpers;
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
    private readonly IRedirectService _redirectService;

    public CommandHandler(
        ILogger<CommandHandler> logger,
        IDockerService dockerService,
        IContainerTracker containerTracker,
        IGitProviderFactory gitProviderFactory,
        IConfigurationManager configurationManager,
        IRedirectService redirectService)
    {
        _logger = logger;
        _dockerService = dockerService;
        _containerTracker = containerTracker;
        _gitProviderFactory = gitProviderFactory;
        _configurationManager = configurationManager;
        _redirectService = redirectService;
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
            _ => RestartAsync(metadata, cancellationToken),
            _ => Task.CompletedTask);
    }

    private async Task RestartAsync(CommandMetadata metadata, CancellationToken cancellationToken)
    {
        DockerContainer? existingContainer = _containerTracker.SingleOrDefault(c =>
            c.PullRequestId == metadata.PullRequestId);

        IGitProvider gitProvider = GetGitProvider(metadata.GitProvider);

        if (existingContainer is null)
        {
            Log.ContainerNotFound(_logger, metadata.PullRequestId);

            await gitProvider.PostContainerNotFoundMessageAsync(
                IdHelper.GetAzureReposId(metadata),
                metadata.PullRequestId,
                cancellationToken);
            
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

        PreviewEnvironmentConfiguration? configuration = _configurationManager
            .GetConfigurationById(newContainer.InternalBuildId);

        if (configuration is null)
        {
            Log.ConfigurationNotFound(_logger, newContainer.InternalBuildId);
            return;
        }

        string smallId = newContainer.ContainerId[..12];
        
        Uri address =
            new($"{configuration.Deployment.ContainerHostAddress}:{newContainer.Port}");
        
        address = _redirectService.Add(
            smallId,
            address,
            metadata.Host);
        
        await gitProvider.PostPreviewAvailableMessageAsync(
            newContainer.InternalBuildId,
            metadata.PullRequestId,
            address,
            cancellationToken);
    }

    private IGitProvider GetGitProvider(string providerType)
    {
        GitProvider gitProviderType = providerType.GetGitProviderFromString();

        return _gitProviderFactory.CreateProvider(gitProviderType);
    }
}