using Microsoft.Extensions.Options;
using PreviewEnvironments.API.Contracts.AzureDevOps.v2;
using PreviewEnvironments.API.Extensions;
using PreviewEnvironments.API.Mappers;
using PreviewEnvironments.Application.Models;
using PreviewEnvironments.Application.Services.Abstractions;

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
                IPreviewEnvironmentManager previewEnvironmentManager,
                IOptions<ApplicationConfiguration> options) =>
            {
                options.Apply(contract);

                await previewEnvironmentManager.BuildCompleteAsync(contract.ToModel());

                return Results.NoContent();
            })
            .WithName(Name)
            .WithOpenApi();

        return app;
    }
}
