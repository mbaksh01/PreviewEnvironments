namespace PreviewEnvironments.API.Endpoints.Meta;

public static class MetaEndpointExtensions
{
    public static IEndpointRouteBuilder MapMetaEndpoints(this IEndpointRouteBuilder app)
    {
        return app.MapRoot();
    }
}