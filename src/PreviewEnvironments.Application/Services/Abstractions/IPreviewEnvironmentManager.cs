﻿using PreviewEnvironments.Application.Models.AzureDevOps.Builds;
using PreviewEnvironments.Application.Models.AzureDevOps.PullRequests;

namespace PreviewEnvironments.Application.Services.Abstractions;

public interface IPreviewEnvironmentManager : IAsyncDisposable
{
    /// <summary>
    /// Initialises this service and its dependencies.
    /// </summary>
    /// <returns></returns>
    Task InitialiseAsync(CancellationToken cancellationToken = default);

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