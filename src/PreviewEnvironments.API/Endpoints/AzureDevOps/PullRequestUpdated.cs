using PreviewEnvironments.API.Contracts.AzureDevOps.v1;
using PreviewEnvironments.API.Mappers;
using PreviewEnvironments.Application.Services.Abstractions;

namespace PreviewEnvironments.API.Endpoints.AzureDevOps;

public static class PullRequestUpdated
{
    public const string Name = "PullRequestUpdated";

    public static IEndpointRouteBuilder MapPullRequestUpdated(this IEndpointRouteBuilder app)
    {
        _ = app.MapPost(
            Constants.EndPoints.VSTFS.PullRequestUpdated,
            async (
                PullRequestUpdatedContract contract,
                IPreviewEnvironmentManager previewEnvironmentManager) =>
            {
                await previewEnvironmentManager.PullRequestUpdatedAsync(contract.ToModel());

                return Results.NoContent();
            })
            .WithName(Name)
            .WithOpenApi();

        return app;
    }
}
