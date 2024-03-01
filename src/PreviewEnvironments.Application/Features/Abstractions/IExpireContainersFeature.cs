namespace PreviewEnvironments.Application.Features.Abstractions;

public interface IExpireContainersFeature
{
    /// <summary>
    /// Checks if any containers have reached their expiration time. If they
    /// have then they are stopped.
    /// </summary>
    /// <param name="cancellationToken">
    /// Cancellation token used to stop this task.
    /// </param>
    /// <returns></returns>
    Task ExpireContainersAsync(CancellationToken cancellationToken = default);
}