using PreviewEnvironments.Application.Models.Docker;

namespace PreviewEnvironments.Application.Services.Abstractions;

public interface IDockerService : IAsyncDisposable
{
    /// <summary>
    /// Event used to notify other services that a container has expired.
    /// </summary>
    event Func<DockerContainer, Task>? ContainerExpiredAsync;

    /// <summary>
    /// Initialises this service.
    /// </summary>
    /// <param name="cancellationToken">
    /// Cancellation token used to stop this task.
    /// </param>
    /// <returns>
    /// A <see cref="bool"/> indicating the state of the initialisation.
    /// <see langword="true"/> if succeeded otherwise <see langword="false"/>.
    /// </returns>
    Task<bool> InitialiseAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs a container using the given parameters.
    /// </summary>
    /// <param name="imageName">Name of the image to use.</param>
    /// <param name="imageTag">Tag of the image to use.</param>
    /// <param name="buildDefinitionId">
    /// Build definition linked to this container. Used for tracking purposes.
    /// </param>
    /// <param name="registry">Registry to pull the image from.</param>
    /// <param name="exposedPort">Port exposed in the docker image.</param>
    /// <param name="cancellationToken">
    /// Cancellation token used to stop this task.
    /// </param>
    /// <returns>
    /// An <see cref="int"/> containing the public exposed port of the started
    /// container. If the container failed to start then 0 is returned.
    /// </returns>
    Task<int> RunContainerAsync(
        string imageName,
        string imageTag,
        int buildDefinitionId,
        string registry = "localhost:5002",
        int exposedPort = 80,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Restarts a container using the given parameters. If the container is not
    /// found then a new container is created and started.
    /// </summary>
    /// <param name="imageName">Name of the image to use.</param>
    /// <param name="imageTag">Tag of the image to use.</param>
    /// <param name="buildDefinitionId">
    /// Build definition linked to this container. Used for tracking purposes.
    /// </param>
    /// <param name="registry">Registry to pull the image from.</param>
    /// <param name="exposedPort">Port exposed in the docker image.</param>
    /// <param name="cancellationToken">
    /// Cancellation token used to stop this task.
    /// </param>
    /// <returns>
    /// An <see cref="int"/> containing the public exposed port of the started
    /// container. If the container failed to start then 0 is returned.
    /// </returns>
    Task<int> RestartContainerAsync(
        string imageName,
        string imageTag,
        int buildDefinitionId,
        string registry = "localhost:5002",
        int exposedPort = 80,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Checks if any containers have reached their expiration time. If they
    /// have then they are stopped.
    /// </summary>
    /// <param name="cancellationToken">
    /// Cancellation token used to stop this task.
    /// </param>
    /// <returns></returns>
    Task ExpireContainersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops a container and removes it and its image.
    /// </summary>
    /// <param name="pullRequestId">Pull request id linked to the container.</param>
    /// <param name="cancellationToken">
    /// Cancellation token used to stop this task.
    /// </param>
    /// <returns>
    /// A <see cref="bool"/> indicating the status of the task.
    /// <see langword="true"/> if successfully stopped and removed otherwise
    /// <see langword="false"/>.
    /// </returns>
    Task<bool> StopAndRemoveContainerAsync(int pullRequestId, CancellationToken cancellationToken = default);
}
