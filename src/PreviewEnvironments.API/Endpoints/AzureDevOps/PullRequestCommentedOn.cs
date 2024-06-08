using PreviewEnvironments.API.Mappers;
using PreviewEnvironments.Application.Services.Abstractions;
using PreviewEnvironments.Contracts.AzureDevOps.v2;

namespace PreviewEnvironments.API.Endpoints.AzureDevOps;

public static class PullRequestCommentedOn
{
    public const string Name = "PullRequestCommentOn";

    public static IEndpointRouteBuilder MapPullRequestCommentOn(this IEndpointRouteBuilder app)
    {
        _ = app.MapPost(
                Constants.EndPoints.VSTFS.PullRequestCommentOn,
                async (
                    PullRequestCommentedOnContract contract,
                    HttpContext context,
                    ICommandHandler commandHandler) =>
                {
                    await commandHandler.HandleAsync(
                        contract.Resource.Comment.Content,
                        contract.ToMetadata().WithHost(context.Request));

                    return Results.NoContent();
                })
            .WithName(Name)
            .WithOpenApi();

        return app;
    }
}