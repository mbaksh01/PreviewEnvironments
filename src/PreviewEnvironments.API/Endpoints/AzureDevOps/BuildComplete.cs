using PreviewEnvironments.API.Mappers;
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
                IPreviewEnvironmentManager previewEnvironmentManager) =>
            {
                await previewEnvironmentManager.BuildCompleteAsync(contract.ToModel());

                return Results.NoContent();
            })
            .WithName(Name)
            .WithOpenApi();

        return app;
    }
}
