namespace PreviewEnvironments.Application.Models.AzureDevOps.Builds;

/// <summary>
/// Model containing all the information required to handle complete builds.
/// </summary>
public sealed class BuildComplete
{
    /// <summary>
    /// Id used to identify a configuration file.
    /// </summary>
    public required string InternalBuildId { get; set; }
    
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
    public required int PullRequestId { get; set; }

    /// <summary>
    /// The host address this application is running on.
    /// </summary>
    public Uri Host { get; set; } = default!;
}
