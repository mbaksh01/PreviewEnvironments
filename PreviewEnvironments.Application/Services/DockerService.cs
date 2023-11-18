﻿using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.Logging;
using PreviewEnvironments.Application.Extensions;
using PreviewEnvironments.Application.Models.Docker;
using PreviewEnvironments.Application.Services.Abstractions;
using System.Collections.Concurrent;
using System.Net;

namespace PreviewEnvironments.Application.Services;

/**
 * TODO: Add container lifetime. - Done
 * TODO: Clean up on PR close/abandon. - Done
 * TODO: Fix disposal on application shutdown. - Done
 * TODO: Look at adding memory caps to containers.
 * TODO: Move configuration to app settings.
 * TODO: Post message to say container has expired.
 */
internal class DockerService : IDockerService, IAsyncDisposable
{
    private readonly ILogger<DockerService> _logger;
    private readonly DockerClient _dockerClient;
    private readonly Progress<JSONMessage> _progress;
    private readonly ConcurrentDictionary<string, DockerContainer> _containers;

    private readonly TimeSpan _containerTimeout = TimeSpan.FromSeconds(10);

    public event Func<DockerContainer, Task>? ContainerExpiredAsync;

    public DockerService(ILogger<DockerService> logger)
    {
        _logger = logger;
        _dockerClient = new DockerClientConfiguration().CreateClient();
        _progress = new Progress<JSONMessage>();
        _containers = new ConcurrentDictionary<string, DockerContainer>();

        _progress.ProgressChanged += Progress_ProgressChanged;
    }

    public async Task<bool> InitialiseAsync(CancellationToken cancellationToken = default)
    {
        string registryVersion = "latest";

        await PullImageAsync("registry", registryVersion, cancellationToken);

        _logger.LogInformation(
            "Docker registry image cloned from docker hub. Version '{registryVersion}'.",
            registryVersion
        );

        IList<ContainerListResponse> containers = await _dockerClient
            .Containers
            .ListContainersAsync(
                new ContainersListParameters
                {
                    All = true,
                },
                cancellationToken
            );

        if (containers.Any(c =>
            c.Names.Contains($"/{Constants.Containers.PreviewImageRegistry}"))
        )
        {
            ContainerListResponse container = containers.Single(c =>
                c.Names.Contains($"/{Constants.Containers.PreviewImageRegistry}")
            );

            if (container.State == "exited")
            {
                await RemoveContainerAsync(container.ID, cancellationToken);
            }
            else
            {
                _ = await StopAndRemoveContainerAsync(container.ID, cancellationToken);
            }
        }

        int registryPort = 5002;

        CreateContainerResponse response = await CreateContainerAsync(
            "registry",
            "latest",
            null,
            5000,
            registryPort,
            Constants.Containers.PreviewImageRegistry,
            cancellationToken
        );

        _logger.LogInformation(
            "Registry container created. Registry container id {registryContainerId}",
            response.ID
        );

        bool started = await StartContainerAsync(response.ID, cancellationToken);

        lock (_containers)
        {
            _ = _containers.TryAdd(response.ID, new DockerContainer
            {
                ContainerId = response.ID,
                ImageName = "registry",
                ImageTag = "latest",
                CanExpire = false,
            });
        }

        if (started)
        {
            _logger.LogInformation(
                "Registry started on port {registryPortNumber}",
                registryPort
            );
            _logger.LogInformation("Docker service initialised.");
        }
        else
        {
            _logger.LogError("Failed to start registry.");
            _logger.LogWarning("Docker service partially initialised.");
        }

        return started;
    }

    public async Task<int> RunContainerAsync(
        string imageName,
        string imageTag,
        string repository = "localhost:5002",
        int exposedPort = 80,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation(
            "Attempting to run container with the following parameters;" +
            " Image: '{imageName}', Tag: '{imageTag}', Repository: '{repository}'," +
            " Exposed port: '{exposedPort}'.",
            imageName,
            imageTag,
            repository,
            exposedPort
        );

        imageName = imageName.ToLower();

        int port = Random.Shared.Next(10_000, 60_000);

        string containerName = $"{imageName}-{imageTag.ToLower()}-preview";

        await PullImageAsync(
            $"{repository}/{imageName}",
            imageTag,
            cancellationToken
        );

        CreateContainerResponse response = await CreateContainerAsync(
            imageName,
            imageTag,
            repository,
            exposedPort,
            port,
            containerName,
            cancellationToken
        );

        bool started = await StartContainerAsync(response.ID, cancellationToken);

        if (started)
        {
            lock (_containers)
            {
                _ = _containers.TryAdd(response.ID, new DockerContainer
                {
                    ContainerId = response.ID,
                    ImageName = $"{repository}/{imageName}",
                    ImageTag = imageTag,
                    PullRequestId = int.Parse(imageTag.AsSpan(imageTag.IndexOf('-') + 1))
                });
            }

            _logger.LogInformation(
                "Container '{previewEnvName}' started on port {previewEnvPortNumber}",
                containerName,
                port
            );
        }

        return started ? port : 0;
    }

    public async Task<int> RestartContainerAsync(
        string imageName,
        string imageTag,
        string repository = "localhost:5002",
        int exposedPort = 80,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation(
            "Restarting container with the following parameters;" +
            " Image: '{imageName}', Tag: '{imageTag}', Repository: '{repository}'," +
            " Exposed port: '{exposedPort}'",
            imageName,
            imageTag,
            repository,
            exposedPort
        );

        string? containerId;

        lock (_containers)
        {
            containerId = _containers.SingleOrDefault(
                dc =>
                    dc.Value.ImageName == $"{repository}/{imageName.ToLower()}"
                    && dc.Value.ImageTag == imageTag
            ).Key;
        }

        if (containerId is null)
        {
            _logger.LogInformation("Could not find a container which matched the required condition.");

            return await RunContainerAsync(
                imageName,
                imageTag,
                repository,
                exposedPort,
                cancellationToken
            );
        }

        await CleanUpAsync(containerId, cancellationToken);

        return await RunContainerAsync(imageName,
            imageTag,
            repository,
            exposedPort,
            cancellationToken
        );
    }

    public async Task ExpireContainersAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to find and stop expired containers.");

        IEnumerable<DockerContainer> containers;

        lock (_containers)
        {
            containers = _containers
                .Where(c =>
                    c.Value.CreatedTime + _containerTimeout < DateTimeOffset.UtcNow
                    && c.Value.CanExpire
                    && c.Value.Expired is false
                )
                .Select(c => c.Value);
        }

        _logger.LogInformation(
            "Found {containerCount} containers to expire.",
            containers.Count()
        );

        foreach (DockerContainer container in containers)
        {
            container.Expired = await StopContainerAsync(
                container.ContainerId,
                cancellationToken
            );

            if (ContainerExpiredAsync is not null)
            {
                await ContainerExpiredAsync.Invoke(container);
            }
        }
    }

    public Task<bool> StopAndRemoveContainerAsync(int pullRequestId, CancellationToken cancellationToken = default)
    {
        string? containerId;

        lock (_containers)
        {
            containerId = _containers
                .SingleOrDefault(c => c.Value.PullRequestId == pullRequestId)
                .Key;
        }

        _logger.LogInformation(
            "Could not find container to remove. Pull Request Id: {pullRequestId}.",
            pullRequestId
        );

        return containerId is null
            ? Task.FromResult(false)
            : StopAndRemoveContainerAsync(containerId, cancellationToken);
    }

    private void Progress_ProgressChanged(object? sender, JSONMessage e)
    {
        _logger.LogInformation(
            "{timeNano} [{id}] Progress: {progress}, Error Message: {errorMessage}",
            e.TimeNano,
            e.ID,
            e.Progress,
            e.ErrorMessage
        );
    }

    private async Task PullImageAsync(string image, string tag, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Attempting to pull an image. Image: '{image}', Tag: '{tag}'.",
            image,
            tag
        );

        await _dockerClient.Images.CreateImageAsync(
            new ImagesCreateParameters
            {
                FromImage = image,
                Tag = tag,
            },
            new AuthConfig(),
            _progress,
            cancellationToken
        );

        _logger.LogInformation(
            "Pulled image '{image}' with tag '{tag}'.",
            image,
            tag
        );
    }

    private async Task<CreateContainerResponse> CreateContainerAsync(
        string imageName,
        string imageTag,
        string? repository,
        int exposedPort,
        int publicPort,
        string containerName,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation(
            "Attempting to create container. Image: '{image}', Tag: '{tag}'," +
            " Repository: '{repository}', Exposed Port: '{exposedPort}'," +
            " Public Port: '{publicPort}', Name: '{containerName}'",
            imageName,
            imageTag,
            repository,
            exposedPort,
            publicPort,
            containerName
        );

        string fullImageName = string.IsNullOrWhiteSpace(repository)
            ? $"{imageName}:{imageTag}"
            : $"{repository}/{imageName}:{imageTag}";

        CreateContainerParameters parameters = new()
        {
            Image = fullImageName,
            Name = containerName,
            HostConfig = new HostConfig
            {
                PortBindings = new Dictionary<string, IList<PortBinding>>
                {
                    {
                        $"{exposedPort}/tcp",
                        new List<PortBinding>
                        {
                            new PortBinding
                            {
                                HostPort = publicPort.ToString(),
                                HostIP = IPAddress.Any.ToString(),
                            },
                        }
                    }
                },
            },
        };

        int attempt = 1;

        CreateContainerResponse? response;

        while (true)
        {
            try
            {
                _logger.LogInformation("Create container attempt {attempt}.", attempt);

                response = await _dockerClient
                    .Containers
                    .CreateContainerAsync(parameters, cancellationToken);

                break;
            }
            // MAYBE: Maybe toggle this behaviour.
            catch (DockerApiException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
            {
                _logger.LogWarning(
                    ex,
                    "Could not start a container due to a conflict."
                );

                _logger.LogInformation("Attempting to stop and remove conflicting container.");

                // TODO: Move to config.
                if (attempt > 3)
                {
                    throw;
                }

                string containerId = ex.GetContainerId();

                _ = await StopContainerAsync(
                    containerId,
                    cancellationToken
                );

                await RemoveContainerAsync(
                    containerId,
                    cancellationToken
                );

                attempt++;
            }
        }

        _logger.LogInformation(
            "Created container. Image: '{image}', Tag: '{tag}'," +
            " Repository: '{repository}', Exposed Port: '{exposedPort}'," +
            " Public Port: '{publicPort}', Name: '{containerName}'",
            imageName,
            imageTag,
            repository,
            exposedPort,
            publicPort,
            containerName
        );

        return response;
    }

    private async Task<bool> StartContainerAsync(string containerId, CancellationToken cancellationToken = default)
    {
        bool started = await _dockerClient.Containers.StartContainerAsync(
            containerId,
            new ContainerStartParameters(),
            cancellationToken
        );

        if (started)
        {
            _logger.LogInformation(
                "Container started. Container id: {containerId}",
                containerId
            );
        }
        else
        {
            _logger.LogInformation(
                "Container not started. Container id: {containerId}",
                containerId
            );
        }

        return started;
    }

    private async Task<bool> StopContainerAsync(string containerId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Attempting to stop container. Container id {containerId}.",
            containerId
        );

        DockerContainer? dockerContainer;

        lock (_containers)
        {
            _ = _containers.TryGetValue(containerId, out dockerContainer);
        }

        if (dockerContainer?.Expired ?? false)
        {
            _logger.LogInformation(
                "Container already stopped. Container id: {containerId}",
                containerId
            );

            return true;
        }

        bool stopped = await _dockerClient.Containers.StopContainerAsync(
            containerId,
            new ContainerStopParameters
            {
                WaitBeforeKillSeconds = 30,
            },
            cancellationToken
        );

        if (stopped)
        {
            _logger.LogInformation(
                "Stopped container. Container id: {containerId}.",
                containerId
            );
        }
        else
        {
            _logger.LogInformation(
                "Failed to stop container. Container id: {containerId}.",
                containerId
            );
        }

        return stopped;
    }

    private async Task<bool> StopAndRemoveContainerAsync(string containerId, CancellationToken cancellationToken = default)
    {
        try
        {
            bool stopped = await StopContainerAsync(containerId, cancellationToken);

            if (!stopped)
            {
                _logger.LogInformation(
                    "Container not stopped. Not attempting to remove container. Container id: {containerId}.",
                    containerId
                );

                return false;
            }

            await RemoveContainerAsync(containerId, cancellationToken);

            return true;
        }
        catch (DockerApiException ex)
        {
            _logger.LogError(
                ex,
                "An error occurred when trying to remove a container." +
                " Container id: {containerId}, Exception Message: {message}",
                containerId,
                ex.Message
            );

            return false;
        }
    }

    private async Task RemoveImageAsync(string imageName, string imageTag, CancellationToken cancellationToken = default)
    {
        string fullImageName = $"{imageName}:{imageTag}";

        _logger.LogInformation(
            "Attempting to remove image. Image: {fullImageName}.",
            fullImageName
        );

        _ = await _dockerClient.Images.DeleteImageAsync(
            fullImageName,
            new ImageDeleteParameters
            {
                Force = true,
            },
            cancellationToken
        );

        _logger.LogInformation(
            "Successfully removed image. Image: {fullImageName}.",
            fullImageName
        );
    }

    public async Task RemoveContainerAsync(string containerId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
                "Attempting to remove container and volumes. Container id: {containerId}",
                containerId
            );

        await _dockerClient.Containers.RemoveContainerAsync(
            containerId,
            new ContainerRemoveParameters
            {
                RemoveVolumes = true,
            },
            cancellationToken
        );

        _logger.LogInformation(
            "Removed container and volumes. Container id: {containerId}.",
            containerId
        );

        lock (_containers)
        {
            _ = _containers.Remove(containerId, out _);
        }
    }

    private async Task CleanUpAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Performing global clean up.");
        _logger.LogInformation("Attempting to remove all containers linked to this application.");

        ICollection<string> containerIds;

        lock (_containers)
        {
            containerIds = _containers.Keys;
        }

        _logger.LogInformation(
            "Found {containerCount} containers to remove.",
            containerIds.Count
        );

        foreach (string containerId in containerIds)
        {
            await CleanUpAsync(containerId, cancellationToken);
        }
    }

    private async Task CleanUpAsync(string containerId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Cleaning up container. Container id: {containerId}.",
            containerId
        );

        DockerContainer? dockerContainer;

        lock (_containers)
        {
            _ = _containers.TryGetValue(containerId, out dockerContainer);
        }

        bool removed = await StopAndRemoveContainerAsync(containerId, cancellationToken);

        if (removed && dockerContainer is not null)
        {
            // TODO: Remove images from registry as well.
            await RemoveImageAsync(
                dockerContainer.ImageName,
                dockerContainer.ImageTag,
                cancellationToken
            );
        }
        else
        {
            _logger.LogInformation(
                "Not attempting to remove image. Container not removed or" +
                " container image was not found. Container id: {containerId}," +
                " Container removed: {containerRemoved}, Container found:" +
                " {containerFound}.",
                containerId,
                removed,
                dockerContainer is not null
            );
        }
    }

    public async ValueTask DisposeAsync()
    {
        await CleanUpAsync();

        try
        {
            _dockerClient.Dispose();
        }
        catch (ObjectDisposedException)
        {
        }

        _progress.ProgressChanged -= Progress_ProgressChanged;

        GC.SuppressFinalize(this);
    }
}
