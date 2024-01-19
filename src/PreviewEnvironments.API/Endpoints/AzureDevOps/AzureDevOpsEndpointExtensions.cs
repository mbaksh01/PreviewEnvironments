namespace PreviewEnvironments.API.Endpoints.AzureDevOps;

public static class AzureDevOpsEndpointExtensions
{
    public static IEndpointRouteBuilder MapAzureDevOpsEndpoints(this IEndpointRouteBuilder app)
    {
        return app
            .MapBuildComplete()
            .MapPullRequestUpdated();
    }
}
