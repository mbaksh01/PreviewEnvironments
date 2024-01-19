using PreviewEnvironments.API.Endpoints.AzureDevOps;

namespace PreviewEnvironments.API.Endpoints;

public static class EndpointsExtensions
{
    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder app)
    {
        return app.MapAzureDevOpsEndpoints();
    }
}
