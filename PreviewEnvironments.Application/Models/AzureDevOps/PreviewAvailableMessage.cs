namespace PreviewEnvironments.Application.Models.AzureDevOps;

/// <summary>
/// Model used to provide the address of the preview environment to the
/// pull request.
/// </summary>
internal sealed class PreviewAvailableMessage : AzureDevOpsMessage
{
    public required string PreviewEnvironmentAddress { get; set; }
}
