namespace PreviewEnvironments.Application.Models.AzureDevOps.PullRequests;

/// <summary>
/// Model containing all the information required to handle updated pull requests.
/// </summary>
public sealed class PullRequestUpdated
{
    /// <summary>
    /// Id of the pull request. Also known as the pull request number.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Current state of the pull request.
    /// </summary>
    public PullRequestState State { get; set; }
}
