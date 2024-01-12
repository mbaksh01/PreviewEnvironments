namespace PreviewEnvironments.Application.Models.AzureDevOps;

/// <summary>
/// Model used to provide the address of the preview environment to the
/// pull request.
/// </summary>
internal sealed class PreviewAvailableMessage : AzureDevOpsMessage
{
    /// <summary>
    /// Pull request number. 
    /// </summary>
    public required int PullRequestNumber { get; set; }
    
    /// <summary>
    /// Address where the preview environment is currently hosted.
    /// </summary>
    public required string PreviewEnvironmentAddress { get; set; }
}
