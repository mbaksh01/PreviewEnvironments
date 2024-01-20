namespace PreviewEnvironments.API;

public static class Constants
{
    public static class EndPoints
    {
        public static class VSTFS
        {
            public const string BuildComplete = "/vstfs/buildComplete";

            public const string PullRequestUpdated = "/vstfs/pullRequestUpdated";
        }
        
        public static class Meta
        {
            public const string Root = "/";
        }
    }
}
