namespace PreviewEnvironments.API.Endpoints.Environments;

public static class EnvironmentEndpointExtensions
{
    public static IEndpointRouteBuilder MapEnvironmentEndpoints(this IEndpointRouteBuilder app)
    {
        return app
            .MapEnvironmentRedirect();
    }
}