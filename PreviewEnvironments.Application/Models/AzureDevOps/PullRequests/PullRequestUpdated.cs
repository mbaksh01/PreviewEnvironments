namespace PreviewEnvironments.Application.Models.AzureDevOps.PullRequests;

public sealed class PullRequestUpdated
{
    public int Id { get; set; }

    public PullRequestState State { get; set; }
}
