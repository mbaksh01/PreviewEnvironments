namespace PreviewEnvironments.Application.Services;

public interface IApplicationLifetimeService : IAsyncDisposable
{
    Task InitialiseAsync(CancellationToken cancellationToken = default);
}