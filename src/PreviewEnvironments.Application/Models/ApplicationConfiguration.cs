namespace PreviewEnvironments.Application.Models;

/// <summary>
/// Stores the configuration for this application.
/// </summary>
public sealed class ApplicationConfiguration
{
    /// <summary>
    /// Indicates if this application should run a local docker registry.
    /// </summary>
    public bool RunLocalRegistry { get; set; }
    
    /// <summary>
    /// Stores how often the check for timed out containers should happen.
    /// </summary>
    public int ContainerTimeoutIntervalSeconds { get; set; }

    /// <summary>
    /// Relative path to the folder containing the preview environment
    /// configuration files.
    /// </summary>
    public string ConfigurationFolder { get; set; } = string.Empty;
}
