using PreviewEnvironments.Contracts.AzureDevOps.v1;
using PreviewEnvironments.Contracts.AzureDevOps.v2;
using PreviewEnvironments.Application.Models.AzureDevOps.Builds;
using PreviewEnvironments.Application.Models.AzureDevOps.PullRequests;
using System.Diagnostics;
using PreviewEnvironments.Application.Helpers;
using PreviewEnvironments.Application.Models.Commands;

namespace PreviewEnvironments.API.Mappers;

public static class AzureDevOpsContractMapper
{
    public static BuildComplete ToModel(this BuildCompleteContract contract)
    {
        BuildStatus status = contract.Resource.Result switch
        {
            "succeeded" => BuildStatus.Succeeded,
            "partiallySucceeded" => BuildStatus.PartiallySucceeded,
            _ => BuildStatus.Failed,
        };

        return new BuildComplete
        {
            SourceBranch = contract.Resource.SourceBranch,
            BuildStatus = status,
            PullRequestId = contract.Resource.TriggerInfo?.PrNumber ?? 0,
            BuildUrl = contract.Resource.Links.Web.Href,
            InternalBuildId = IdHelper.GetAzurePipelinesId(contract)
        };
    }

    public static BuildComplete WithHost(this BuildComplete buildComplete, HttpRequest request)
    {
        string host = request.Host.Host;
        string scheme = request.Scheme;
        int port = request.Host.Port ?? 80;

        if (request.Headers.TryGetValue("Host", out var hostHeader))
        {
            if (!string.IsNullOrWhiteSpace(hostHeader))
            {
                host = hostHeader!;
            }
        }
        
        if (request.Headers.TryGetValue("X-Forwarded-Scheme", out var schemeHeader))
        {
            if (!string.IsNullOrWhiteSpace(schemeHeader))
            {
                scheme = schemeHeader!;
            }
        }
        
        if (request.Headers.TryGetValue("X-Forwarded-Port", out var portHeader))
        {
            _ = int.TryParse(portHeader, out port);
        }

        UriBuilder hostBuilder = new(scheme, host)
        {
            Port = port
        };

        buildComplete.Host = hostBuilder.Uri;

        return buildComplete;
    }

    public static PullRequestUpdated ToModel(this PullRequestUpdatedContract contract)
    {
        PullRequestState state = contract.Resource.Status switch
        {
            "completed" => PullRequestState.Completed,
            "active" => PullRequestState.Active,
            "abandoned" => PullRequestState.Abandoned,
            _ => throw new UnreachableException(),
        };

        return new PullRequestUpdated
        {
            Id = contract.Resource.PullRequestId,
            State = state,
        };
    }

    public static CommandMetadata ToMetadata(this PullRequestCommentedOnContract contract)
    {
        string[] parts = new Uri(contract.Resource.PullRequest.Repository.RemoteUrl)
            .PathAndQuery
            .Split('/', StringSplitOptions.RemoveEmptyEntries);
        
        return new CommandMetadata
        {
            PullRequestId = contract.Resource.PullRequest.PullRequestId,
            GitProvider = Application.Constants.GitProviders.AzureRepos,
            OrganizationName = parts[0],
            ProjectName = parts[1],
            RepositoryName = contract.Resource.PullRequest.Repository.Name
        };
    }
}
