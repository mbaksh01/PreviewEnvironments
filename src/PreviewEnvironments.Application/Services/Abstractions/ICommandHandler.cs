using PreviewEnvironments.Application.Models.Commands;

namespace PreviewEnvironments.Application.Services.Abstractions;

public interface ICommandHandler
{
    /// <summary>
    /// Handles any incoming commands.
    /// </summary>
    /// <param name="comment">Comment posted to pull request.</param>
    /// <param name="metadata">Metadata linked to this command.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task HandleAsync(
        string comment,
        CommandMetadata metadata,
        CancellationToken cancellationToken = default);
}