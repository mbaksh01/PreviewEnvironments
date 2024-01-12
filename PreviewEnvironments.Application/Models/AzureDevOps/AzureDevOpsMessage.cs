namespace PreviewEnvironments.Application.Models.AzureDevOps;

/// <summary>
/// Base model containing information needed to make any call to the Azure
/// DevOps REST API.
/// </summary>
internal class AzureDevOpsMessage
{
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
    public required string Organization { get; set; }

    /// <summary>
    /// Name of project.
    /// </summary>
    public required string Project { get; set; }

    /// <summary>
    /// Repository id.
    /// </summary>
    public required Guid RepositoryId { get; set; }

    /// <summary>
    /// Access token.
    /// </summary>
    public required string AccessToken { get; set; }
}
