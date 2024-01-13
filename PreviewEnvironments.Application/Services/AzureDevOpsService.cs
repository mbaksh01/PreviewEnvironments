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

internal class AzureDevOpsService : IAzureDevOpsService
{
    private readonly ILogger<AzureDevOpsService> _logger;
    private readonly HttpClient _httpClient;
    private readonly ApplicationConfiguration _configuration;

    public AzureDevOpsService(
        ILogger<AzureDevOpsService> logger,
        IOptions<ApplicationConfiguration> options,
        HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
        _configuration = options.Value;
    }
    
    /// <inheritdoc />
    public async Task PostPreviewAvailableMessageAsync(PreviewAvailableMessage message, CancellationToken cancellationToken = default)
    {
        message.AccessToken = GetAccessToken();
        
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

        _ = await _httpClient.SendAsync(request, cancellationToken);
    }

    /// <inheritdoc />
    public async Task PostExpiredContainerMessageAsync(int pullRequestNumber, CancellationToken cancellationToken = default)
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

        _ = await _httpClient.SendAsync(request, cancellationToken);
    }
    
    /// <summary>
    /// Posts a pull request status to Azure DevOps using the
    /// <paramref name="message"/>.
    /// </summary>
    /// <param name="message">Information about the status.</param>
    /// <param name="cancellationToken">
    /// Cancellation token used to stop this task.
    /// </param>
    public async Task PostPullRequestStatusAsync(PullRequestStatusMessage message, CancellationToken cancellationToken = default)
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
}
