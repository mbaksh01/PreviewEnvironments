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
            "partially succeeded" => BuildStatus.PartiallySucceeded,
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
        return new CommandMetadata
        {
            PullRequestId = contract.Resource.PullRequest.PullRequestId,
            GitProvider = Application.Constants.GitProviders.AzureRepos,
        };
    }
}
