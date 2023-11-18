namespace PreviewEnvironments.Application.Models.AzureDevOps.PullRequests;

internal sealed class PullRequestStatusMessage : AzureDevOpsMessage
{
    public required string BuildPipelineAddress { get; set; }

    public PullRequestStatusState State { get; set; } = PullRequestStatusState.NotSet;

    public int Port { get; set; }
}

public enum PullRequestStatusState
{
    Error,
    Failed,
    NotApplicable,
    NotSet,
    Pending,
    Succeeded,
}
