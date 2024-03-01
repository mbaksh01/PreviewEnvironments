using PreviewEnvironments.Application.Models.AzureDevOps.Builds;

namespace PreviewEnvironments.Application.Features.Abstractions;

public interface IBuildCompleteFeature
{
    /// <summary>
    /// Takes a complete build and starts its associated preview environment.
    /// </summary>
    /// <param name="buildComplete">Information about the complete build.</param>
    /// <param name="cancellationToken">
    /// Cancellation token used to stop this task.
    /// </param>
    /// <returns></returns>
    Task BuildCompleteAsync(
        BuildComplete buildComplete,
        CancellationToken cancellationToken = default);
}