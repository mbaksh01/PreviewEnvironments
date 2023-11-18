using PreviewEnvironments.Application.Models.Docker;

namespace PreviewEnvironments.Application.Services.Abstractions;

public interface IDockerService : IAsyncDisposable
{
    event Func<DockerContainer, Task>? ContainerExpiredAsync;

    Task<bool> InitialiseAsync(CancellationToken cancellationToken = default);

    Task<int> RunContainerAsync(
        string imageName,
        string imageTag,
        string repository = "localhost:5002",
        int exposedPort = 80,
        CancellationToken cancellationToken = default
    );

    Task<int> RestartContainerAsync(
        string imageName,
        string imageTag,
        string repository = "localhost:5002",
        int exposedPort = 80,
        CancellationToken cancellationToken = default
    );

    Task ExpireContainersAsync(CancellationToken cancellationToken = default);

    Task<bool> StopAndRemoveContainerAsync(int pullRequestId, CancellationToken cancellationToken = default);
}
