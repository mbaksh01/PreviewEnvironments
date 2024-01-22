namespace PreviewEnvironments.Application.Models.AzureDevOps.PullRequests;

internal sealed class GetPullRequest : AzureDevOpsMessage
{
    public int PullRequestId { get; set; }
}