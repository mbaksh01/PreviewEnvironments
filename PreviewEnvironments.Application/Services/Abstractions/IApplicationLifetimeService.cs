namespace PreviewEnvironments.Application.Services.Abstractions;

public interface IApplicationLifetimeService : IAsyncDisposable
{
    /// <summary>
    /// Runs core application startup tasks.
    /// </summary>
    /// <param name="cancellationToken">
    /// Cancellation token used to stop this task.
    /// </param>
    /// <returns></returns>
    Task InitialiseAsync(CancellationToken cancellationToken = default);
}