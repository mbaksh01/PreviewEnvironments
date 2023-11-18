using PreviewEnvironments.API.Contracts.AzureDevOps.v2;
using PreviewEnvironments.API.Mappers;
using PreviewEnvironments.Application.Services.Abstractions;

namespace PreviewEnvironments.API.Endpoints.AzureDevOps;

public static class BuildComplete
{
    public const string Name = "BuildComplete";

    public static IEndpointRouteBuilder MapBuildComplete(this IEndpointRouteBuilder app)
    {
        _ = app.MapPost(
            Constants.EndPoints.VSTFS.BuildComplete,
            async (BuildCompleteContract contract, IAzureDevOpsService azureDevOpsService) =>
                {
                    await azureDevOpsService.BuildCompleteAsync(contract.ToModel());

                    return TypedResults.Ok(contract);
                }
            )
            .WithName(Name)
            .WithOpenApi();

        return app;
    }
}
