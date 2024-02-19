namespace PreviewEnvironments.Application;

public static class Constants
{
    public static class EnvVariables
    {
        public const string AzAccessToken = "AzAccessToken";
    }

    public static class AppSettings
    {
        public static class Sections
        {
            public const string Configuration = "Configuration";
        }
    }

    public static class Containers
    {
        public const string PreviewImageRegistry = "preview-images-registry";
    }
    
    public static class BuildServers
    {
        public const string AzurePipelines = "AzurePipelines";

        public static readonly string[] AllBuildServers =
        [
            AzurePipelines,
        ];
    }
    
    public static class GitProviders
    {
        public const string AzureRepos = "AzureRepos";
        
        public static readonly string[] AllGitProviders =
        [
            AzureRepos,
        ];
    }
}
