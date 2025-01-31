using PreviewEnvironments.API.Endpoints.AzureDevOps;
using PreviewEnvironments.API.Endpoints.Environments;
using PreviewEnvironments.API.Endpoints.Meta;

namespace PreviewEnvironments.API.Endpoints;

public static class EndpointsExtensions
{
    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder app)
    {
        return app
            .MapMetaEndpoints()
            .MapAzureDevOpsEndpoints()
            .MapEnvironmentEndpoints();
    }
}
