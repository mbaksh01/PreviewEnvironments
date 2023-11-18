namespace PreviewEnvironments.Application.Models.AzureDevOps;

internal sealed class PreviewAvailableMessage : AzureDevOpsMessage
{
    public required string PreviewEnvironmentAddress { get; set; }
}
