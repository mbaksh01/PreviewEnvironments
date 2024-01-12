using Microsoft.Extensions.Logging;
using PreviewEnvironments.Application.Extensions;
using PreviewEnvironments.Application.Models;
using PreviewEnvironments.Application.Models.AzureDevOps;
using PreviewEnvironments.Application.Models.AzureDevOps.Builds;
using PreviewEnvironments.Application.Models.AzureDevOps.PullRequests;
using PreviewEnvironments.Application.Models.Docker;
using PreviewEnvironments.Application.Services.Abstractions;
using System.Diagnostics;
using Microsoft.Extensions.Options;

namespace PreviewEnvironments.Application.Services;

internal class AzureDevOpsService : IAzureDevOpsService, IDisposable
{
    private readonly ILogger<AzureDevOpsService> _logger;
    private readonly IDockerService _dockerService;
    private readonly HttpClient _httpClient;
    private readonly ApplicationConfiguration _configuration;

    public AzureDevOpsService(
        ILogger<AzureDevOpsService> logger,
        IDockerService dockerService,
        HttpClient httpClient,
        IOptions<ApplicationConfiguration> options
    )
    {
        _logger = logger;
        _dockerService = dockerService;
        _httpClient = httpClient;
        _configuration = options.Value;

        _dockerService.ContainerExpiredAsync += ContainerExpiredAsync;
    }

    /// <inheritdoc />
    public async Task BuildCompleteAsync(BuildComplete buildComplete, CancellationToken cancellationToken = default)
    {
        if (!buildComplete.SourceBranch.StartsWith("refs/pull"))
        {
            return;
        }

        if (buildComplete.BuildStatus is not BuildStatus.Succeeded)
        {
            return;
        }

        try
        {
            await PostPullRequestStatus(
                CreateStatusMessage(
                    buildComplete,
                    PullRequestStatusState.Pending
                ),
                cancellationToken
            );
            
            SupportedBuildDefinition? supportedBuildDefinition = _configuration
                .AzureDevOps
                .SupportedBuildDefinitions
                .FirstOrDefault(sbd => sbd.BuildDefinitionId == buildComplete.BuildDefinitionId);
            
            if (supportedBuildDefinition is null)
            {
                return;
            }
            
            // ASSUMPTION: Assuming that the tag is the pr number with 'pr-' prefixed.
            int port = await _dockerService.RestartContainerAsync(
                supportedBuildDefinition.ImageName,
                $"pr-{buildComplete.PrNumber}",
                supportedBuildDefinition.BuildDefinitionId,
                supportedBuildDefinition.DockerRegistry,
                cancellationToken: cancellationToken
            );

            string accessToken = GetAccessToken();
            
            await PostPreviewAvailableMessage(new()
            {
                Organization = _configuration.AzureDevOps.Organization,
                Project = _configuration.AzureDevOps.ProjectName,
                RepositoryId = _configuration.AzureDevOps.RepositoryId,
                AccessToken = accessToken,
                PullRequestNumber = buildComplete.PrNumber,
                PreviewEnvironmentAddress = $"{_configuration.Scheme}://{_configuration.Host}:{port}",
            });

            await PostPullRequestStatus(
                CreateStatusMessage(
                    buildComplete,
                    PullRequestStatusState.Succeeded,
                    port
                ),
                cancellationToken
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "An error occurred when trying to run a container."
            );

            await PostPullRequestStatus(
                CreateStatusMessage(
                    buildComplete,
                    PullRequestStatusState.Failed
                ),
                cancellationToken
            );
        }
    }
    
    /// <inheritdoc />
    public ValueTask PullRequestUpdatedAsync(PullRequestUpdated pullRequestUpdated, CancellationToken cancellationToken = default)
    {
        return pullRequestUpdated.State switch
        {
            PullRequestState.Completed or PullRequestState.Abandoned => PullRequestClosedAsync(pullRequestUpdated.Id, cancellationToken),
            _ => ValueTask.CompletedTask
        };
    }
    
    /// <summary>
    /// Posts the <see cref="PreviewAvailableMessage"/> to Azure DevOps.
    /// </summary>
    /// <param name="message"></param>
    private async Task PostPreviewAvailableMessage(PreviewAvailableMessage message)
    {
        UriBuilder builder = new()
        {
            Host = message.Host,
            Scheme = message.Scheme,
            Path = $"{message.Organization}/{message.Project}/_apis/git/repositories/{message.RepositoryId}/pullRequests/{message.PullRequestNumber}/threads",
            Query = "api-version=7.0"
        };

        string accessToken = GetAccessToken();

        PullRequestThread thread = new()
        {
            Comments = new Comment[]
            {
                new()
                {
                    CommentType = "system",
                    Content = $"Preview environment available at [{message.PreviewEnvironmentAddress}]({message.PreviewEnvironmentAddress}).",
                },
            },
            Status = "closed",
        };

        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, builder.ToString())
            .WithAuthorization(accessToken)
            .WithBody(thread);

        _ = await _httpClient.SendAsync(request);
    }

    /// <summary>
    /// Posts a message to the pull request stating a container has been stopped.
    /// </summary>
    /// <param name="pullRequestNumber">Pull request number to post to.</param>
    private async Task PostExpiredContainerMessage(int pullRequestNumber)
    {
        // https://learn.microsoft.com/en-us/rest/api/azure/devops/git/pull-request-threads/create?tabs=HTTP

        UriBuilder builder = new()
        {
            Host = _configuration.AzureDevOps.Host,
            Scheme = _configuration.AzureDevOps.Scheme,
            Path = $"{_configuration.AzureDevOps.Organization}/{_configuration.AzureDevOps.ProjectName}/_apis/git/repositories/{_configuration.AzureDevOps.RepositoryId}/pullRequests/{pullRequestNumber}/threads",
            Query = "api-version=7.0"
        };

        string accessToken = GetAccessToken();

        PullRequestThread thread = new()
        {
            Comments =
            [
                new Comment
                {
                    CommentType = "system",
                    Content = """
                        Preview environment has been stopped to save resources.
                        To restart the container, re-queue the build.
                        If your containers stop too early consider increasing the container timeout time.
                        """,
                },
            ],
            Status = "closed",
        };
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, builder.ToString())
            .WithAuthorization(accessToken)
            .WithBody(thread);

        _ = await _httpClient.SendAsync(request);
    }
    
    /// <summary>
    /// Posts a pull request status to Azure DevOps using the
    /// <paramref name="message"/>.
    /// </summary>
    /// <param name="message">Information about the status.</param>
    /// <param name="cancellationToken">
    /// Cancellation token used to stop this task.
    /// </param>
    private async Task PostPullRequestStatus(PullRequestStatusMessage message, CancellationToken cancellationToken = default)
    {
        // https://learn.microsoft.com/en-us/rest/api/azure/devops/git/pull-request-statuses/create?tabs=HTTP

        UriBuilder builder = new()
        {
            Host = message.Host,
            Scheme = message.Scheme,
            Path = $"{message.Organization}/{message.Project}/_apis/git/repositories/{message.RepositoryId}/pullRequests/{message.PullRequestNumber}/statuses",
            Query = "api-version=7.0",
        };

        PullRequestStatus status = new()
        {
            State = GetStatus(message.State),
            Description = GetStatusDescription(message.State),
            TargetUrl = message.BuildPipelineAddress,
            Context = new Context
            {
                Genre = "preview-environments",
                Name = "deployment-status"
            },
        };

        using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, builder.ToString())
            .WithAuthorization(message.AccessToken)
            .WithBody(status);

        using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);

        try
        {
            _ = response.EnsureSuccessStatusCode();
            
            _logger.LogInformation(
                "Successfully posed status as '{pullRequestStatus}'.",
                message.State);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to post status.");
            _logger.LogError(
                "Azure DevOps Api Response: {apiResponse}.",
                await response.Content.ReadAsStringAsync(cancellationToken)
            );
        }
    }

    /// <summary>
    /// Event handler for <see cref="IDockerService.ContainerExpiredAsync"/>.
    /// </summary>
    /// <param name="container"></param>
    private async Task ContainerExpiredAsync(DockerContainer container)
    {
        _logger.LogInformation("Container Expired event fired.");

        await PostExpiredContainerMessage(container.PullRequestId);
    }

    /// <summary>
    /// Stops the container linked to the provided
    /// <paramref name="pullRequestId"/>.
    /// </summary>
    /// <param name="pullRequestId">Id of pull request that was closed.</param>
    /// <param name="cancellationToken">
    /// Cancellation token used to stop this task.
    /// </param>
    private async ValueTask PullRequestClosedAsync(int pullRequestId, CancellationToken cancellationToken = default)
    {
        bool response = await _dockerService.StopAndRemoveContainerAsync(
            pullRequestId,
            cancellationToken
        );

        if (response)
        {
            _logger.LogInformation(
                "Successfully closed pull request {pullRequestId}.",
                pullRequestId
            );
            
            return;
        }
        
        _logger.LogInformation(
            "Failed to close pull request {pullRequestId}.",
            pullRequestId
        );
    }

    /// <summary>
    /// Converts a <see cref="PullRequestStatus"/> to its corresponding string
    /// value.
    /// </summary>
    /// <param name="state"></param>
    /// <returns>The <paramref name="state"/> in its string format.</returns>
    /// <exception cref="UnreachableException"></exception>
    private static string GetStatus(PullRequestStatusState state)
    {
        return state switch
        {
            PullRequestStatusState.Error => "error",
            PullRequestStatusState.Failed => "failed",
            PullRequestStatusState.Succeeded => "succeeded",
            PullRequestStatusState.NotSet => "notSet",
            PullRequestStatusState.Pending => "pending",
            PullRequestStatusState.NotApplicable => "notApplicable",
            _ => throw new UnreachableException()
        };
    }

    /// <summary>
    /// Converts a <see cref="PullRequestStatus"/> to its description.
    /// </summary>
    /// <param name="state"></param>
    /// <returns>The description for the <paramref name="state"/>.</returns>
    /// <exception cref="UnreachableException"></exception>
    private static string GetStatusDescription(PullRequestStatusState state)
    {
        return state switch
        {
            PullRequestStatusState.Error => "An error occurred when trying to deploy the preview environment.",
            PullRequestStatusState.Failed => "Failed to deploy preview environment.",
            PullRequestStatusState.Succeeded => "Successfully deployed preview environment.",
            PullRequestStatusState.NotSet => "Preview environment not deployed.",
            PullRequestStatusState.Pending => "Deploying preview environment.",
            PullRequestStatusState.NotApplicable => "Preview environments are not supported for this pull request.",
            _ => throw new UnreachableException()
        };
    }

    /// <summary>
    /// Creates a <see cref="PullRequestStatusMessage"/> from the given
    /// parameters and the current configuration.
    /// </summary>
    /// <param name="buildComplete"></param>
    /// <param name="state"></param>
    /// <param name="port"></param>
    /// <returns>A correctly initialised <see cref="PullRequestStatusMessage"/>.</returns>
    private PullRequestStatusMessage CreateStatusMessage(
        BuildComplete buildComplete,
        PullRequestStatusState state,
        int port = 0)
    {
        string accessToken = GetAccessToken();
        
        return new PullRequestStatusMessage
        {
            Organization = _configuration.AzureDevOps.Organization,
            Project = _configuration.AzureDevOps.ProjectName,
            RepositoryId = _configuration.AzureDevOps.RepositoryId,
            AccessToken = accessToken,
            PullRequestNumber = buildComplete.PrNumber,
            BuildPipelineAddress = buildComplete.BuildUrl.ToString(),
            State = state,
            Port = port
        };
    }

    /// <summary>
    /// Attempts to get the access token from the environment variables with the
    /// token provided in the configuration acting as a fallback.
    /// </summary>
    /// <returns>The Azure DevOps access token.</returns>
    /// <exception cref="Exception">
    /// Throw then the access token could not be identified.
    /// </exception>
    private string GetAccessToken()
    {
        string? accessToken = EnvironmentHelper
            .GetAzAccessToken()
            .WithFallback(_configuration.AzureDevOps.AzAccessToken);

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            throw new Exception(
                $"""
                 {Constants.EnvVariables.AzAccessToken} was not present in the
                 environmental variables or appsettings.json. This token is
                 required to interact with Azure DevOps APIs.
                 """
            );
        }

        return accessToken;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _dockerService.ContainerExpiredAsync -= ContainerExpiredAsync;
    }
}
