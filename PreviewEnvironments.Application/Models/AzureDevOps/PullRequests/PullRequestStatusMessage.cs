namespace PreviewEnvironments.Application.Models.AzureDevOps.PullRequests;

/// <summary>
/// Contains the required information to post a pull request status to Azure
/// DevOps.
/// </summary>
internal sealed class PullRequestStatusMessage : AzureDevOpsMessage
{
    /// <summary>
    /// Link to build associated with this status.
    /// </summary>
    public required string BuildPipelineAddress { get; set; }

    /// <summary>
    /// Current state of this status.
    /// </summary>
    public PullRequestStatusState State { get; set; } = PullRequestStatusState.NotSet;

    /// <summary>
    /// Port of the preview environment.
    /// </summary>
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
