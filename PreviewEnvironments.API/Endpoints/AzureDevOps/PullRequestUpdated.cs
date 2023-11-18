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
            async (PullRequestUpdatedContract contract, IAzureDevOpsService azureDevOpsService) =>
            {
                await azureDevOpsService.PullRequestUpdatedAsync(contract.ToModel());

                return TypedResults.Ok(contract);
            })
            .WithName(Name)
            .WithOpenApi();

        return app;
    }
}
