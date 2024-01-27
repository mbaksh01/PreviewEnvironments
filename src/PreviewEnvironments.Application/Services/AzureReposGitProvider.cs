using System.Diagnostics;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PreviewEnvironments.Application.Extensions;
using PreviewEnvironments.Application.Models;
using PreviewEnvironments.Application.Models.AzureDevOps.Contracts;
using PreviewEnvironments.Application.Models.AzureDevOps.PullRequests;
using PreviewEnvironments.Application.Services.Abstractions;

namespace PreviewEnvironments.Application.Services;

internal sealed partial class AzureReposGitProvider : IGitProvider
{
    private readonly ILogger<AzureReposGitProvider> _logger;
    private readonly IConfigurationManager _configurationManager;
    private readonly HttpClient _httpClient;
    private readonly ApplicationConfiguration _configuration;
    
    private static readonly PullRequestThreadRequest ExpiredContainerThread = new()
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

    public AzureReposGitProvider(
        ILogger<AzureReposGitProvider> logger,
        IOptions<ApplicationConfiguration> options,
        IConfigurationManager configurationManager,
        HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
        _configurationManager = configurationManager;
        _configuration = options.Value;
    }
    
    /// <inheritdoc />
    public async Task PostPreviewAvailableMessageAsync(
        string internalBuildId,
        int pullRequestId,
        Uri containerAddress,
        CancellationToken cancellationToken = default)
    {
        AzureRepos? configuration = _configurationManager.GetConfigurationByBuildId(internalBuildId)?.AzureRepos;

        if (configuration is null)
        {
            // TODO: Log error
            return;
        }
        
        UriBuilder builder = new(configuration.BaseAddress)
        {
            Path = $"{configuration.OrganizationName}/{configuration.ProjectName}/_apis/git/repositories/{configuration.RepositoryId}/pullRequests/{pullRequestId}/threads",
            Query = "api-version=7.0"
        };

        PullRequestThreadRequest thread = new()
        {
            Comments =
            [
                new Comment
                {
                    CommentType = "system",
                    Content = $"Preview environment available at [{containerAddress}]({containerAddress}).",
                }
            ],
            Status = "closed",
        };

        using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, builder.ToString())
            .WithBasicAuthorization(GetAccessToken(internalBuildId))
            .WithJsonBody(thread);

        using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);

        try
        {
            _ = response.EnsureSuccessStatusCode();

            Log.PostedPreviewAvailableMessage(_logger, pullRequestId);
        }
        catch (Exception ex)
        {
            Log.PostedPreviewAvailableFailed(_logger, ex, pullRequestId);
            
            string apiResponse =
                await response.Content.ReadAsStringAsync(cancellationToken);
            
            Log.AzureDevOpsApiResponseError(_logger, apiResponse);
        }
    }

    /// <inheritdoc />
    public async Task PostExpiredContainerMessageAsync(
        string internalBuildId,
        int pullRequestId,
        CancellationToken cancellationToken = default)
    {
        AzureRepos? configuration = _configurationManager.GetConfigurationByBuildId(internalBuildId)?.AzureRepos;

        if (configuration is null)
        {
            // TODO: Log error
            return;
        }

        UriBuilder builder = new(configuration.BaseAddress)
        {
            Path = $"{configuration.OrganizationName}/{configuration.ProjectName}/_apis/git/repositories/{configuration.RepositoryId}/pullRequests/{pullRequestId}/threads",
            Query = "api-version=7.0"
        };
        
        using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, builder.Uri)
            .WithBasicAuthorization(GetAccessToken(internalBuildId))
            .WithJsonBody(ExpiredContainerThread);
        
        using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);

        try
        {
            _ = response.EnsureSuccessStatusCode();
            
            Log.PostedExpiredContainerMessage(_logger, pullRequestId);
        }
        catch (Exception ex)
        {
            Log.PostedStatusFailed(_logger, ex);

            string apiResponse =
                await response.Content.ReadAsStringAsync(cancellationToken);
            
            Log.AzureDevOpsApiResponseError(_logger, apiResponse);
        }
    }
    
    /// <inheritdoc />
    public async Task PostPullRequestStatusAsync(
        string internalBuildId,
        int pullRequestId,
        PullRequestStatusState state,
        CancellationToken cancellationToken = default)
    {
        AzureRepos? configuration = _configurationManager.GetConfigurationByBuildId(internalBuildId)?.AzureRepos;

        if (configuration is null)
        {
            // TODO: Log error
            return;
        }
        
        UriBuilder builder = new(configuration.BaseAddress)
        {
            Path = $"{configuration.OrganizationName}/{configuration.ProjectName}/_apis/git/repositories/{configuration.RepositoryId}/pullRequests/{pullRequestId}/statuses",
            Query = "api-version=7.0",
        };

        PullRequestStatusRequest status = new()
        {
            State = GetStatus(state),
            Description = GetStatusDescription(state),
            TargetUrl = string.Empty,
            Context = new Context
            {
                Genre = "preview-environments",
                Name = "deployment-status"
            },
        };

        using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, builder.Uri)
            .WithBasicAuthorization(GetAccessToken(internalBuildId))
            .WithJsonBody(status);

        using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);

        try
        {
            _ = response.EnsureSuccessStatusCode();
            
            Log.PostedStatusSuccessfully(_logger, state);
        }
        catch (Exception ex)
        {
            Log.PostedStatusFailed(_logger, ex);

            string apiResponse =
                await response.Content.ReadAsStringAsync(cancellationToken);
            
            Log.AzureDevOpsApiResponseError(_logger, apiResponse);
        }
    }

    /// <inheritdoc />
    public async Task<PullRequestResponse?> GetPullRequestById(
        string internalBuildId,
        int pullRequestId,
        CancellationToken cancellationToken = default)
    {
        AzureRepos? configuration = _configurationManager.GetConfigurationByBuildId(internalBuildId)?.AzureRepos;

        if (configuration is null)
        {
            // TODO: Log error
            return null;
        }

        UriBuilder builder = new(configuration.BaseAddress)
        {
            Path = $"{configuration.OrganizationName}/{configuration.ProjectName}/_apis/git/pullrequests/{pullRequestId}",
            Query = "api-version=7.0"
        };

        using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, builder.Uri)
            .WithBasicAuthorization(GetAccessToken(internalBuildId));

        using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);

        try
        {
            _ = response.EnsureSuccessStatusCode();

            PullRequestResponse? pullRequest = await response.Content
                .ReadFromJsonAsync<PullRequestResponse>(cancellationToken);

            Log.GetPullRequestByIdSucceeded(_logger, pullRequestId);

            return pullRequest;
        }
        catch (Exception ex)
        {
            Log.GetPullRequestByIdFailed(_logger, ex, pullRequestId);

            string apiResponse =
                await response.Content.ReadAsStringAsync(cancellationToken);

            Log.AzureDevOpsApiResponseError(_logger, apiResponse);

            return null;
        }
    }

    /// <summary>
    /// Converts a <see cref="PullRequestStatusRequest"/> to its corresponding
    /// string value.
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
    /// Converts a <see cref="PullRequestStatusRequest"/> to its description.
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
    private string GetAccessToken(string internalBuildId)
    {
        AzureRepos? configuration = _configurationManager
            .GetConfigurationByBuildId(internalBuildId)
            ?.AzureRepos;
        
        string? accessToken = EnvironmentHelper
            .GetAzAccessToken()
            .WithFallback(configuration?.PersonalAccessToken);

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
