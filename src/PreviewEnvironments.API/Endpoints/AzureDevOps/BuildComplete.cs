using PreviewEnvironments.API.Endpoints.Environments;
using PreviewEnvironments.API.Mappers;
using PreviewEnvironments.Application.Features.Abstractions;
using PreviewEnvironments.Contracts.AzureDevOps.v2;

namespace PreviewEnvironments.API.Endpoints.AzureDevOps;

public static class BuildComplete
{
    public const string Name = "BuildComplete";

    public static IEndpointRouteBuilder MapBuildComplete(this IEndpointRouteBuilder app)
    {
        _ = app.MapPost(
            Constants.EndPoints.VSTFS.BuildComplete,
            async (
                BuildCompleteContract contract,
                HttpContext context,
                IBuildCompleteFeature buildCompleteFeature) =>
            {
                string? smallId = await buildCompleteFeature.BuildCompleteAsync(contract
                    .ToModel()
                    .WithHost(context.Request));

                return string.IsNullOrWhiteSpace(smallId)
                    ? Results.NoContent()
                    : Results.CreatedAtRoute(EnvironmentRedirect.Name, new { id = smallId });
            })
            .WithName(Name)
            .WithOpenApi();

        return app;
    }
}
