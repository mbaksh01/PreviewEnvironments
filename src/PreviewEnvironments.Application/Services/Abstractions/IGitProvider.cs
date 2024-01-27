﻿using PreviewEnvironments.Application.Models.AzureDevOps;
using PreviewEnvironments.Application.Models.AzureDevOps.Contracts;
using PreviewEnvironments.Application.Models.AzureDevOps.PullRequests;

namespace PreviewEnvironments.Application.Services.Abstractions;

internal interface IGitProvider
{
    /// <summary>
    /// Posts the <see cref="PreviewAvailableMessage"/> to Azure DevOps.
    /// </summary>
    /// <param name="internalBuildId">
    /// Id used to get the correct configuration file.
    /// </param>
    /// <param name="pullRequestId">Id of pull request.</param>
    /// <param name="containerAddress">
    /// Address where container can be accessed.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancellation token used to stop this task.
    /// </param>
    Task PostPreviewAvailableMessageAsync(
        string internalBuildId,
        int pullRequestId,
        Uri containerAddress,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Posts a message to the pull request stating a container has been stopped.
    /// </summary>
    /// <param name="internalBuildId">
    /// Id used to get the correct configuration file.
    /// </param>
    /// <param name="pullRequestId">Pull request number to post to.</param>
    /// <param name="cancellationToken">
    /// Cancellation token used to stop this task.
    /// </param>
    Task PostExpiredContainerMessageAsync(
        string internalBuildId,
        int pullRequestId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Posts a pull request status to the pull request with id
    /// <paramref name="pullRequestId"/>.
    /// </summary>
    /// <param name="internalBuildId">
    /// Id used to get the correct configuration file.
    /// </param>
    /// <param name="pullRequestId">Id of pull request.</param>
    /// <param name="state">
    /// The current state of the pull request status.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancellation token used to stop this task.
    /// </param>
    Task PostPullRequestStatusAsync(
        string internalBuildId,
        int pullRequestId,
        PullRequestStatusState state,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets information about a pull request by its id.
    /// </summary>
    /// <param name="internalBuildId">
    /// Id used to get the correct configuration file.
    /// </param>
    /// <param name="pullRequestId">Id of pull request.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>
    /// The pull request linked to the <paramref name="pullRequestId"/>,
    /// otherwise <see langword="null"/> if and error occurred.
    /// </returns>
    Task<PullRequestResponse?> GetPullRequestById(
        string internalBuildId,
        int pullRequestId,
        CancellationToken cancellationToken = default);
}