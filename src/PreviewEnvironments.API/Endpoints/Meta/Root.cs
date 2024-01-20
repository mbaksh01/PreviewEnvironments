namespace PreviewEnvironments.API.Endpoints.Meta;

public static class Root
{
    private const string Name = "Root";

    public static IEndpointRouteBuilder MapRoot(this IEndpointRouteBuilder app)
    {
        _ = app.MapGet(
                Constants.EndPoints.Meta.Root,
                () => Results.Ok("Preview environments running."))
            .WithName(Name)
            .ExcludeFromDescription();

        return app;
    }
}