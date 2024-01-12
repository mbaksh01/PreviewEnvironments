namespace PreviewEnvironments.Application.Models.AzureDevOps;

/// <summary>
/// Base model containing information needed to make any call to the Azure
/// DevOps REST API.
/// </summary>
internal class AzureDevOpsMessage
{
    public string Host { get; set; } = "dev.azure.com";

    public string Scheme { get; set; } = "https";

    public required string Organization { get; set; }

    public required string Project { get; set; }

    public required Guid RepositoryId { get; set; }

    public required int PullRequestNumber { get; set; }

    public required string AccessToken { get; set; }
}
