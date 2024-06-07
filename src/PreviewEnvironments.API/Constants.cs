namespace PreviewEnvironments.API;

public static class Constants
{
    public static class EndPoints
    {
        public static class VSTFS
        {
            public const string BuildComplete = "/vstfs/buildComplete";

            public const string PullRequestUpdated = "/vstfs/pullRequestUpdated";

            public const string PullRequestCommentOn = "/vstfs/pullRequestCommentedOn";
        }
        
        public static class Meta
        {
            public const string Root = "/";
        }
        
        public static class Containers
        {
            public const string EnvironmentRedirect = "/environments/{id}";
        }
    }
}
