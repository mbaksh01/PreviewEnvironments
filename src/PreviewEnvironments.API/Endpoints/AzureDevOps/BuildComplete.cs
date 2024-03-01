using PreviewEnvironments.API.Mappers;
using PreviewEnvironments.Application.Features.Abstractions;
using PreviewEnvironments.Application.Services.Abstractions;
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
                IBuildCompleteFeature buildCompleteFeature) =>
            {
                await buildCompleteFeature.BuildCompleteAsync(contract.ToModel());

                return Results.NoContent();
            })
            .WithName(Name)
            .WithOpenApi();

        return app;
    }
}
