namespace PreviewEnvironments.Application.Models.AzureDevOps;

/// <summary>
/// Base model containing information needed to make any call to the Azure
/// DevOps REST API.
/// </summary>
internal class AzureDevOpsMessage
{
    public AzureDevOpsMessage()
    {
        
    }
    
    /// <summary>
    /// Azure Devops host. Added to support Azure DevOps Server.
    /// </summary>
    public string Host { get; set; } = "dev.azure.com";

    /// <summary>
    /// Host scheme.
    /// </summary>
    public string Scheme { get; set; } = "https";

    /// <summary>
    /// Name of organization.
    /// </summary>
    public string Organization { get; set; } = string.Empty;

    /// <summary>
    /// Name of project.
    /// </summary>
    public string Project { get; set; } = string.Empty;

    /// <summary>
    /// Repository id.
    /// </summary>
    public Guid RepositoryId { get; set; }

    /// <summary>
    /// Access token.
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;
}
