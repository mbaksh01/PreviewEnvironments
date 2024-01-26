namespace PreviewEnvironments.Application.Models;

public class PreviewEnvironmentConfiguration
{
    public PreviewEnvironmentConfiguration()
    {
        GitProvider = SetGitProvider();
        BuildServerProvider = SetBuildServerProvider();
    }

    public string GitProvider { get; set; }

    public string BuildServerProvider { get; set; }

    public Deployment Deployment { get; set; } = new();

    public AzureRepos? AzureRepos { get; set; }

    public AzurePipelines? AzurePipelines { get; set; }

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
            return "AzurePipelines";
        }

        return "Unknown";
    }
}

public class Deployment
{
    public string ContainerHostAddress { get; set; } = string.Empty;

    public string ImageName { get; set; } = string.Empty;

    public string ImageRegistry { get; set; } = string.Empty;

    public int[] AllowedDeploymentPorts { get; set; } = Array.Empty<int>();

    public int ContainerTimeoutSeconds { get; set; }

    public int CreateContainerRetryCount { get; set; }
}

public class AzureRepos
{
    public string OrganizationName { get; set; } = string.Empty;

    public string ProjectName { get; set; } = string.Empty;

    public string PersonalAccessToken { get; set; } = string.Empty;
}

public class AzurePipelines
{
    public string ProjectName { get; set; } = string.Empty;
    
    public int BuildDefinitionId { get; set; }
}