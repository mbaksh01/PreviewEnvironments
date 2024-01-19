namespace PreviewEnvironments.Application.Models.AzureDevOps.Builds;

/// <summary>
/// Model containing all the information required to handle complete builds.
/// </summary>
public sealed class BuildComplete
{
    /// <summary>
    /// Id of build definition.
    /// </summary>
    public required int BuildDefinitionId { get; set; }
    
    /// <summary>
    /// Url to build.
    /// </summary>
    public required Uri BuildUrl { get; set; }

    /// <summary>
    /// Status of the build.
    /// </summary>
    public required BuildStatus BuildStatus { get; set; }

    /// <summary>
    /// Name of the branch that was built.
    /// </summary>
    public required string SourceBranch { get; set; }
    
    /// <summary>
    /// The PR number associated with this build.
    /// </summary>
    public required int PullRequestNumber { get; set; }
}
