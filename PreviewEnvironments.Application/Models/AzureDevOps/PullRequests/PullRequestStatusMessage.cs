namespace PreviewEnvironments.Application.Models.AzureDevOps.PullRequests;

/// <summary>
/// Contains the required information to post a pull request status to Azure
/// DevOps.
/// </summary>
internal sealed class PullRequestStatusMessage : AzureDevOpsMessage
{
    
    /// <summary>
    /// Pull request number. 
    /// </summary>
    public int PullRequestNumber { get; set; }

    /// <summary>
    /// Link to build associated with this status.
    /// </summary>
    public string BuildPipelineAddress { get; set; } = string.Empty;

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
