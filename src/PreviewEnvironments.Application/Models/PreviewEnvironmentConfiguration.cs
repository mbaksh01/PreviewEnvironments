namespace PreviewEnvironments.Application.Models;

public class PreviewEnvironmentConfiguration
{
    public PreviewEnvironmentConfiguration()
    {
        GitProvider = SetGitProvider();
        BuildServer = SetBuildServerProvider();
    }

    /// <summary>
    /// Name of the Git provider. This will be used to identify the correct API
    /// to call when making pull request contributions.
    /// </summary>
    public string GitProvider { get; init; }

    /// <summary>
    /// Name of the build server provider. This will be used to uniquely
    /// identify and incoming webhook with a configuration file.
    /// </summary>
    public string BuildServer { get; init; }

    /// <summary>
    /// Stores configuration related to the deployment.
    /// </summary>
    public Deployment Deployment { get; init; } = new();

    /// <summary>
    /// Stores configuration related to Azure Repos.
    /// </summary>
    public AzureRepos? AzureRepos { get; init; }

    /// <summary>
    /// Stores configuration related to Azure Pipelines.
    /// </summary>
    public AzurePipelines? AzurePipelines { get; init; }

    private string SetGitProvider()
    {
        if (AzureRepos is not null)
        {
            return "AzureRepos";
        }

        return "Unknown";
    }

    private string SetBuildServerProvider()
    {
        if (AzurePipelines is not null)
        {
            return Constants.BuildServers.AzurePipelines;
        }

        return "Unknown";
    }
}

public class Deployment
{
    /// <summary>
    /// The host address where the deployed container will be accessible.
    /// </summary>
    public string ContainerHostAddress { get; set; } = string.Empty;

    /// <summary>
    /// Name of the docker image produced by the build.
    /// </summary>
    public string ImageName { get; set; } = string.Empty;

    /// <summary>
    /// Registry where the docker image is stored.
    /// </summary>
    public string ImageRegistry { get; set; } = string.Empty;

    /// <summary>
    /// A list of allowed port the container can run on.
    /// </summary>
    /// <remarks>
    /// When empty, a port between 10,000 and 60,000 is chosen at random.
    /// </remarks>
    public int[] AllowedDeploymentPorts { get; set; } = Array.Empty<int>();

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

/// <summary>
/// Model containing information about Azure Repos.
/// </summary>
public class AzureRepos
{
    /// <summary>
    /// Address where the repo can be access from.
    /// </summary>
    public Uri BaseAddress { get; set; } = new("https://dev.azure.com");
    
    /// <summary>
    /// Name of the organization containing the repository where pull request
    /// messages and statuses will be posted.
    /// </summary>
    public string OrganizationName { get; set; } = string.Empty;

    /// <summary>
    /// Name of the project containing the repository where pull request
    /// messages and statuses will be posted.
    /// </summary>
    public string ProjectName { get; set; } = string.Empty;

    /// <summary>
    /// Name of the repository containing the pull requests.
    /// </summary>
    public string RepositoryName { get; set; } = string.Empty;

    /// <summary>
    /// Access token which will be used as authorization when calling the Azure
    /// DevOps REST APIs. This value should be supplied through the use of the
    /// AzAccessToken environmental variable and is ignored if the AzAccessToken
    /// environmental variable is present.
    ///
    /// PAT scopes: Code Read & Write, Code Status
    /// </summary>
    public string PersonalAccessToken { get; set; } = string.Empty;
}

/// <summary>
/// Model containing information about Azure Pipelines.
/// </summary>
public class AzurePipelines : IBuildPipeline
{
    /// <summary>
    /// Name of the project containing the pipeline which will trigger the
    /// webhook.
    /// </summary>
    public string ProjectName { get; set; } = string.Empty;
    
    /// <summary>
    /// Build definition id of the pipeline which will trigger the webhook.
    /// </summary>
    public int BuildDefinitionId { get; set; }

    public string GetId()
    {
        return $"{ProjectName}-{BuildDefinitionId}";
    }
}

public interface IBuildPipeline
{
    string GetId();
}