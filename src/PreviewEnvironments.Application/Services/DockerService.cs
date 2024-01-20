using System.Net;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PreviewEnvironments.Application.Extensions;
using PreviewEnvironments.Application.Models;
using PreviewEnvironments.Application.Models.Docker;
using PreviewEnvironments.Application.Services.Abstractions;

namespace PreviewEnvironments.Application.Services;

/**
 * TODO: Look at adding memory caps to containers.
 */
internal sealed partial class DockerService : IDockerService
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
        
        var os = Environment.OSVersion;
        
        if (os.Platform == PlatformID.Unix) {
            _dockerClient = new DockerClientConfiguration (
                    new Uri ("unix:///var/run/docker.sock"))
                .CreateClient ();
        } else if (os.Platform == PlatformID.Win32NT) {
            _dockerClient = new DockerClientConfiguration (
                    new Uri ("npipe://./pipe/docker_engine"))
                .CreateClient ();
        }
    }

    /// <inheritdoc />
    public async Task<DockerContainer?> InitialiseAsync(CancellationToken cancellationToken = default)
    {
        if (_configuration.RunLocalRegistry is false)
        {
            return null;
        }
        
        ContainerListResponse? container = await _dockerClient
            .GetContainerByName(Constants.Containers.PreviewImageRegistry);

        if (container is not null)
        {
            if (container.State == "exited")
            {
                await RemoveContainerAsync(container.ID, cancellationToken);
            }
            else
            {
                _ = await StopAndRemoveContainerAsync(container.ID, cancellationToken);
            }
        }
        
        const string registryVersion = "latest";
        
        await PullImageAsync("registry", registryVersion, cancellationToken);

        Log.PulledDockerImage(_logger, "Docker Hub", "registry", registryVersion);

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

        bool started = await StartContainerAsync(response.ID, cancellationToken);

        if (started is false)
        {
            _logger.LogError("Failed to start registry.");
            _logger.LogWarning("Docker service partially initialised.");

            return null;
        }

        _logger.LogInformation(
            "Registry started on port {RegistryPortNumber}",
            registryPort
        );
            
        _logger.LogInformation("Docker service initialised.");
        
        DockerContainer registryContainer = new()
        {
            ContainerId = response.ID,
            ImageName = "registry",
            ImageTag = "latest",
            CanExpire = false,
        };
        
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
        Log.AttemptingToRunContainer(_logger, registry, imageName, imageTag, publicPort);

        imageName = imageName.ToLower();

        SupportedBuildDefinition? supportedBuildDefinition = _configuration.GetBuildDefinition(buildDefinitionId);

        if (supportedBuildDefinition is null)
        {
            Log.BuildDefinitionNotFound(_logger, buildDefinitionId);
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
        
        Log.RestartingContainer(_logger, registry, imageName, existingContainer.ImageTag, existingContainer.Port);

        _ = await StopAndRemoveContainerAsync(existingContainer.ContainerId, cancellationToken);

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
            ContainerListResponse? container = await _dockerClient
                .GetContainerById(containerId);

            if (container is null)
            {
                Log.ContainerNotFound(_logger, containerId);
                return false;
            }
            
            bool stopped = await StopContainerAsync(containerId, cancellationToken);

            if (!stopped)
            {
                Log.ContainerNotStopped(_logger, containerId);
                return false;
            }

            await RemoveContainerAsync(containerId, cancellationToken);
            await RemoveImageAsync(container.Image, cancellationToken);
            
            return true;
        }
        catch (DockerApiException ex)
        {
            Log.ErrorRemovingContainer(_logger, ex, containerId);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> StopContainerAsync(string containerId, CancellationToken cancellationToken = default)
    {
        Log.AttemptingToStopContainer(_logger, containerId);

        ContainerListResponse? container = await _dockerClient
            .GetContainerById(containerId);

        if (container is null)
        {
            Log.ContainerNotFound(_logger, containerId);
            return false;
        }
        
        if (container.State is not "running")
        {
            Log.ContainerStopped(_logger, containerId);
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
            Log.ContainerStopped(_logger, containerId);
        }
        else
        {
            Log.ErrorStoppingContainer(_logger, containerId);
        }

        return stopped;
    }
    
    private void Progress_ProgressChanged(object? sender, JSONMessage e)
    {
        Log.DockerProgressChanged(_logger, e.TimeNano, e.ID, e.Progress, e.ErrorMessage);
    }

    private async Task PullImageAsync(string image, string tag, CancellationToken cancellationToken = default)
    {
        Log.AttemptingToPullDockerImage(_logger, string.Empty, image, tag);

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

        Log.PulledDockerImage(_logger, string.Empty, image, tag);
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
        Log.AttemptingToCreateContainer(_logger, registry, imageName, imageTag, publicPort, containerName);
        
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
                Log.CreateContainerAttempt(_logger, attempt);
                
                response = await _dockerClient
                    .Containers
                    .CreateContainerAsync(parameters, cancellationToken);

                break;
            }
            catch (DockerApiException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
            {
                Log.ErrorCreatingContainerConflict(_logger, ex);
                Log.AttemptingToStopAndRemoveConflictContainer(_logger);
                
                if (attempt > _configuration.Docker.CreateContainerRetryCount)
                {
                    throw new Exception("Maximum number of attempts reached.", ex);
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

        Log.CreatedContainer(_logger, registry, imageName, imageTag, publicPort, containerName);

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
            Log.ContainerStarted(_logger, containerId);
        }
        else
        {
            Log.ContainerNotStarted(_logger, containerId);
        }

        return started;
    }
    
    private async Task RemoveImageAsync(string image, CancellationToken cancellationToken = default)
    {
        Log.AttemptingToRemoveImage(_logger, image);
        
        _ = await _dockerClient.Images.DeleteImageAsync(
            image,
            new ImageDeleteParameters
            {
                Force = true,
            },
            cancellationToken
        );
        
        // TODO: Remove image from source.
    
        Log.ImageRemoved(_logger, image);
    }

    private async Task RemoveContainerAsync(string containerId, CancellationToken cancellationToken = default)
    {
        Log.AttemptingToRemoveContainer(_logger, containerId);

        await _dockerClient.Containers.RemoveContainerAsync(
            containerId,
            new ContainerRemoveParameters
            {
                RemoveVolumes = true,
            },
            cancellationToken
        );

        Log.RemovedContainer(_logger, containerId);
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
