namespace PreviewEnvironments.Application.Services.Abstractions;

public interface IApplicationLifetimeService : IAsyncDisposable
{
    Task InitialiseAsync(CancellationToken cancellationToken = default);
}