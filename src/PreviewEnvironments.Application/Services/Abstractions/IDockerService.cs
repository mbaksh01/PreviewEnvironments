using PreviewEnvironments.Application.Models.Docker;

namespace PreviewEnvironments.Application.Services.Abstractions;

internal interface IDockerService : IDisposable
{
    /// <summary>
    /// Initialises this service.
    /// </summary>
    /// <param name="cancellationToken">
    /// Cancellation token used to stop this task.
    /// </param>
    /// <returns>
    /// A <see cref="DockerContainer"/> containing information about the started container.
    /// <see langword="null"/> if the initialisation failed.
    /// </returns>
    Task<DockerContainer?> InitialiseAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs a container using the given parameters.
    /// </summary>
    /// <param name="imageName">Name of the image to use.</param>
    /// <param name="imageTag">Tag of the image to use.</param>
    /// <param name="buildDefinitionId">
    /// Build definition linked to this container. Used for tracking purposes.
    /// </param>
    /// <param name="publicPort">Port number which can be used to access the container.</param>
    /// <param name="registry">Registry to pull the image from.</param>
    /// <param name="exposedPort">Port exposed in the docker image.</param>
    /// <param name="cancellationToken">
    /// Cancellation token used to stop this task.
    /// </param>
    /// <returns>
    /// An <see cref="DockerContainer"/> containing information about that
    /// started container. <see langword="null"/> when the container failed to
    /// start.
    /// </returns>
    Task<DockerContainer?> RunContainerAsync(
        string imageName,
        string imageTag,
        int buildDefinitionId,
        int publicPort,
        string registry = "localhost:5002",
        int exposedPort = 80,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Restarts a container using the given parameters. If the container is not
    /// found then a new container is created and started.
    /// </summary>
    /// <param name="existingContainer">
    /// Container that should be teared down.
    /// </param>
    /// <param name="exposedPort">Port exposed in the docker image.</param>
    /// <param name="cancellationToken">
    /// Cancellation token used to stop this task.
    /// </param>
    /// <returns>
    /// An <see cref="DockerContainer"/> containing information about that
    /// started container. <see langword="null"/> when the container failed to
    /// start.
    /// </returns>
    Task<DockerContainer?> RestartContainerAsync(
        DockerContainer existingContainer,
        int exposedPort = 80,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Stops a container and removes it and its image.
    /// </summary>
    /// <param name="containerId">Id of container to stop and remove.</param>
    /// <param name="cancellationToken">
    /// Cancellation token used to stop this task.
    /// </param>
    /// <returns>
    /// A <see cref="bool"/> indicating the status of the task.
    /// <see langword="true"/> if successfully stopped and removed otherwise
    /// <see langword="false"/>.
    /// </returns>
    Task<bool> StopAndRemoveContainerAsync(string containerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops the container linked to the <paramref name="containerId"/>.
    /// </summary>
    /// <param name="containerId">Id of container to stop.</param>
    /// <param name="cancellationToken">
    /// Cancellation token used to stop this task.
    /// </param>
    /// <returns>
    /// A <see cref="bool"/> indicating the state of this task.
    /// <see langword="true"/> if the container was stopped otherwise
    /// <see langword="false"/>.
    /// </returns>
    Task<bool> StopContainerAsync(
        string containerId,
        CancellationToken cancellationToken = default);
}
