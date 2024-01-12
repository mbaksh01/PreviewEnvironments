namespace PreviewEnvironments.Application.Models;

/// <summary>
/// Stores the configuration for this application.
/// </summary>
public sealed class ApplicationConfiguration
{
    /// <summary>
    /// Stores configuration related to Azure DevOps.
    /// </summary>
    public AzureDevOpsConfiguration AzureDevOps { get; set; } = new();

    /// <summary>
    /// Stores configuration related to Docker.
    /// </summary>
    public DockerConfiguration Docker { get; set; } = new();
    
    /// <summary>
    /// Indicates if this application should run a local docker registry.
    /// </summary>
    public bool RunLocalRegistry { get; set; }
    
    /// <summary>
    /// Stores how often the check for timed out containers should happen.
    /// </summary>
    public int ContainerTimeoutIntervalSeconds { get; set; }

    /// <summary>
    /// The host which will be running the containers. This value will be posted
    /// to pull request and will act as a way of users finding the container.
    /// </summary>
    public string Host { get; set; } = "localhost";

    /// <summary>
    /// Scheme of the <see cref="Host"/>.
    /// </summary>
    public string Scheme { get; set; } = "http";
}

public sealed class DockerConfiguration
{
    /// <summary>
    /// Stores how long a container should run for before timing out.
    /// </summary>
    public int ContainerTimeoutSeconds { get; set; }
    
    /// <summary>
    /// Stores how many reties should take place in the scenario that a
    /// container fails to start.
    /// </summary>
    public int CreateContainerRetryCount { get; set; }
}

public sealed class AzureDevOpsConfiguration
{
    /// <summary>
    /// Scheme of the <see cref="Host"/>.
    /// </summary>
    public string Scheme { get; set; } = "https";

    /// <summary>
    /// Host address of Azure DevOps.
    /// </summary>
    public string Host { get; set; } = "dev.azure.com";
    
    /// <summary>
    /// Name of the organization containing the
    /// <see cref="SupportedBuildDefinitions"/>.
    /// </summary>
    public string Organization { get; set; } = string.Empty;

    /// <summary>
    /// Name of the project current project. This value is changed at runtime
    /// and values supplied in the configuration will be ignored.
    /// </summary>
    public string ProjectName { get; set; } = string.Empty;

    /// <summary>
    /// Id of the current repository. This value is changed at runtime and
    /// values supplied in the configuration will be ignored.
    /// </summary>
    public Guid RepositoryId { get; set; }

    /// <summary>
    /// Access token which will be used as authorization when calling the Azure
    /// DevOps REST APIs. This value should be supplied through the use of the
    /// AzAccessToken environmental variable and is ignored if the AzAccessToken
    /// environmental variable is present.
    ///
    /// PAT scopes: Code Read & Write, Code Status
    /// </summary>
    public string? AzAccessToken { get; set; }

    /// <summary>
    /// List of builds which are support by this application.
    /// </summary>
    public SupportedBuildDefinition[] SupportedBuildDefinitions { get; set; } =
        Array.Empty<SupportedBuildDefinition>();
}

/// <summary>
/// Model containing information on how to start a preview environment for a
/// given build definition.
/// </summary>
public sealed class SupportedBuildDefinition
{
    /// <summary>
    /// Name of project containing the build definition.
    /// </summary>
    public string ProjectName { get; set; } = string.Empty;
    
    /// <summary>
    /// Build definition id.
    /// </summary>
    public int BuildDefinitionId { get; set; }

    /// <summary>
    /// Name of docker image produced by the build.
    /// </summary>
    public string ImageName { get; set; } = string.Empty;

    /// <summary>
    /// Registry where the docker image will be store.
    /// </summary>
    public string DockerRegistry { get; set; } = string.Empty;

    /// <summary>
    /// A list of allowed port the container can run on. When empty, a port
    /// between 10,000 and 60,000 is chosen at random.
    /// </summary>
    public int[] AllowedImagePorts { get; set; } = Array.Empty<int>();
}
