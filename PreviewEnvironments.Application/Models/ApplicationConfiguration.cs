namespace PreviewEnvironments.Application.Models;

public sealed class ApplicationConfiguration
{
    public AzureDevOpsConfiguration AzureDevOps { get; set; } = new();

    public DockerConfiguration Docker { get; set; } = new();
    
    public bool RunLocalRegistry { get; set; }
    
    public int ContainerTimeoutIntervalSeconds { get; set; }
}

public sealed class DockerConfiguration
{
    public int ContainerTimeoutSeconds { get; set; }
}

public sealed class AzureDevOpsConfiguration
{
    public string Scheme { get; set; } = "https";

    public string Host { get; set; } = "dev.azure.com";
    
    public string Organization { get; set; } = string.Empty;

    public string Project { get; set; } = string.Empty;

    public Guid RepositoryId { get; set; }

    public string? AzAccessToken { get; set; }

    public SupportedBuildDefinition[] SupportedBuildDefinitions { get; set; } =
        Array.Empty<SupportedBuildDefinition>();
}

public sealed class SupportedBuildDefinition
{
    public int BuildDefinitionId { get; set; }

    public string ImageName { get; set; } = string.Empty;

    public string DockerRegistry { get; set; } = string.Empty;

    public int[] AllowedImagePorts { get; set; } = Array.Empty<int>();
}
