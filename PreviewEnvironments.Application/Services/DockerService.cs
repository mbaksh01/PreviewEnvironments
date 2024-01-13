using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.Logging;
using PreviewEnvironments.Application.Extensions;
using PreviewEnvironments.Application.Models.Docker;
using PreviewEnvironments.Application.Services.Abstractions;
using System.Net;
using Microsoft.Extensions.Options;
using PreviewEnvironments.Application.Models;

namespace PreviewEnvironments.Application.Services;

/**
 * TODO: Look at adding memory caps to containers.
 * TODO: Post message to say container has expired.
 */
internal class DockerService : IDockerService
{
    private readonly ILogger<DockerService> _logger;
    private readonly ApplicationConfiguration _configuration;
    private readonly DockerClient _dockerClient;
    private readonly Progress<JSONMessage> _progress;
    
    public DockerService(ILogger<DockerService> logger, IOptions<ApplicationConfiguration> configuration)
    {
        _logger = logger;
        _configuration = configuration.Value;
        _dockerClient = new DockerClientConfiguration().CreateClient();
        _progress = new Progress<JSONMessage>();

        _progress.ProgressChanged += Progress_ProgressChanged;
    }

    /// <inheritdoc />
    public async Task<DockerContainer?> InitialiseAsync(CancellationToken cancellationToken = default)
    {
        if (_configuration.RunLocalRegistry is false)
        {
            return null;
        }
        
        const string registryVersion = "latest";

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

        const int registryPort = 5002;

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

        DockerContainer registryContainer = new()
        {
            ContainerId = response.ID,
            ImageName = "registry",
            ImageTag = "latest",
            CanExpire = false,
        };

        if (started is false)
        {
            _logger.LogError("Failed to start registry.");
            _logger.LogWarning("Docker service partially initialised.");

            return null;
        }

        _logger.LogInformation(
            "Registry started on port {registryPortNumber}",
            registryPort
        );
            
        _logger.LogInformation("Docker service initialised.");
        
        return registryContainer;
    }

    /// <inheritdoc />
    public async Task<DockerContainer?> RunContainerAsync(
        string imageName,
        string imageTag,
        int buildDefinitionId,
        int publicPort,
        string registry = "localhost:5002",
        int exposedPort = 80,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogDebug(
            "Attempting to run container with the following parameters;" +
            " Image: '{imageName}', Tag: '{imageTag}', Registry: '{registry}'," +
            " Exposed port: '{exposedPort}'.",
            imageName,
            imageTag,
            registry,
            exposedPort
        );

        imageName = imageName.ToLower();

        SupportedBuildDefinition? supportedBuildDefinition = _configuration.GetBuildDefinition(buildDefinitionId);

        if (supportedBuildDefinition is null)
        {
            _logger.LogError(
                "No build definition with id '{buildDefinitionId}' was found.",
                buildDefinitionId);
            
            return null;
        }

        string containerName = $"{imageName}-{imageTag.ToLower()}-preview";

        await PullImageAsync(
            $"{registry}/{imageName}",
            imageTag,
            cancellationToken
        );

        CreateContainerResponse response = await CreateContainerAsync(
            imageName,
            imageTag,
            registry,
            exposedPort,
            publicPort,
            containerName,
            cancellationToken
        );

        bool started = await StartContainerAsync(response.ID, cancellationToken);

        DockerContainer startedContainer = new()
        {
            ContainerId = response.ID,
            ImageName = $"{registry}/{imageName}",
            ImageTag = imageTag,
            PullRequestId = int.Parse(imageTag.AsSpan(imageTag.IndexOf('-') + 1)),
            BuildDefinitionId = buildDefinitionId,
            Port = publicPort
        };
        
        return started ? startedContainer : null;
    }

    /// <inheritdoc />
    public async Task<DockerContainer?> RestartContainerAsync(
        DockerContainer existingContainer,
        int exposedPort = 80,
        CancellationToken cancellationToken = default
    )
    {
        string[] splitImageName = existingContainer.ImageName.Split('/');

        string registry = string.Empty;
        string imageName;
        
        if (splitImageName.Length == 1)
        {
            imageName = splitImageName[0];
        }
        else
        {
            registry = splitImageName[0];
            imageName = splitImageName[1];
        }
        
        _logger.LogDebug(
            "Restarting container with the following parameters;" +
            " Image: '{imageName}', Tag: '{imageTag}', Registry: '{registry}'," +
            " Exposed port: '{exposedPort}'",
            imageName,
            existingContainer.ImageTag,
            registry,
            exposedPort
        );

        await CleanUpAsync(existingContainer.ContainerId, cancellationToken);

        return await RunContainerAsync(
            imageName,
            existingContainer.ImageTag,
            existingContainer.BuildDefinitionId,
            existingContainer.Port,
            registry,
            exposedPort,
            cancellationToken
        );
    }
    
    /// <inheritdoc />
    public async Task<bool> StopAndRemoveContainerAsync(string containerId, CancellationToken cancellationToken = default)
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

    /// <inheritdoc />
    public async Task<bool> StopContainerAsync(string containerId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug(
            "Attempting to stop container. Container id {containerId}.",
            containerId
        );

        // DockerContainer? dockerContainer;
        //
        // lock (_containers)
        // {
        //     _ = _containers.TryGetValue(containerId, out dockerContainer);
        // }
        //
        // if (dockerContainer?.Expired ?? false)
        // {
        //     _logger.LogInformation(
        //         "Container already stopped. Container id: {containerId}",
        //         containerId
        //     );
        //
        //     return true;
        // }

        IList<ContainerListResponse> containers = await _dockerClient
            .Containers
            .ListContainersAsync(new ContainersListParameters
            {
                All = true,
                Filters = new Dictionary<string, IDictionary<string, bool>>
                {
                    ["id"] = new Dictionary<string, bool>
                    {
                        [containerId] = true,
                    }
                }
            }, cancellationToken);

        if (containers.Any() is false)
        {
            _logger.LogDebug(
                "No containers found mating the following criteria. -a -f \"id={containerId}\"",
                containerId);
            
            return false;
        }
        
        if (containers.Single().State is not "running")
        {
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
    
    private void Progress_ProgressChanged(object? sender, JSONMessage e)
    {
        _logger.LogDebug(
            "{timeNano} [{id}] Progress: {progress}, Error Message: {errorMessage}",
            e.TimeNano,
            e.ID,
            e.Progress,
            e.ErrorMessage
        );
    }

    private async Task PullImageAsync(string image, string tag, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug(
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
        string? registry,
        int exposedPort,
        int publicPort,
        string containerName,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogDebug(
            "Attempting to create container. Image: '{image}', Tag: '{tag}'," +
            " Registry: '{registry}', Exposed Port: '{exposedPort}'," +
            " Public Port: '{publicPort}', Name: '{containerName}'",
            imageName,
            imageTag,
            registry,
            exposedPort,
            publicPort,
            containerName
        );

        string fullImageName = string.IsNullOrWhiteSpace(registry)
            ? $"{imageName}:{imageTag}"
            : $"{registry}/{imageName}:{imageTag}";

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
                _logger.LogDebug("Create container attempt {attempt}.", attempt);

                response = await _dockerClient
                    .Containers
                    .CreateContainerAsync(parameters, cancellationToken);

                break;
            }
            catch (DockerApiException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
            {
                _logger.LogDebug(
                    ex,
                    "Could not start a container due to a conflict."
                );

                _logger.LogDebug("Attempting to stop and remove conflicting container.");
                
                if (attempt > _configuration.Docker.CreateContainerRetryCount)
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
            " Registry: '{registry}', Exposed Port: '{exposedPort}'," +
            " Public Port: '{publicPort}', Name: '{containerName}'",
            imageName,
            imageTag,
            registry,
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
    
    private async Task RemoveImageAsync(string imageName, string imageTag, CancellationToken cancellationToken = default)
    {
        string fullImageName = $"{imageName}:{imageTag}";
    
        _logger.LogDebug(
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

    private async Task RemoveContainerAsync(string containerId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug(
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
    }

    private async Task CleanUpAsync(string containerId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Cleaning up container. Container id: {containerId}.",
            containerId
        );

        // DockerContainer? dockerContainer;
        //
        // lock (_containers)
        // {
        //     _ = _containers.TryGetValue(containerId, out dockerContainer);
        // }

        bool removed = await StopAndRemoveContainerAsync(containerId, cancellationToken);

        // if (removed && dockerContainer is not null)
        // {
        //     // TODO: Remove images from registry as well.
        //     await RemoveImageAsync(
        //         dockerContainer.ImageName,
        //         dockerContainer.ImageTag,
        //         cancellationToken
        //     );
        // }
        // else
        // {
        //     _logger.LogInformation(
        //         "Not attempting to remove image. Container not removed or" +
        //         " container image was not found. Container id: {containerId}," +
        //         " Container removed: {containerRemoved}, Container found:" +
        //         " {containerFound}.",
        //         containerId,
        //         removed,
        //         dockerContainer is not null
        //     );
        // }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        try
        {
            _dockerClient.Dispose();
        }
        catch (ObjectDisposedException)
        {
        }

        _progress.ProgressChanged -= Progress_ProgressChanged;
    }
}
