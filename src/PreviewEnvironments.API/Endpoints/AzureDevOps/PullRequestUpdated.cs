using PreviewEnvironments.API.Mappers;
using PreviewEnvironments.Application.Features.Abstractions;
using PreviewEnvironments.Application.Services.Abstractions;
using PreviewEnvironments.Contracts.AzureDevOps.v1;

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
                IPullRequestUpdatedFeature pullRequestUpdatedFeature) =>
            {
                await pullRequestUpdatedFeature.PullRequestUpdatedAsync(contract.ToModel());

                return Results.NoContent();
            })
            .WithName(Name)
            .WithOpenApi();

        return app;
    }
}
