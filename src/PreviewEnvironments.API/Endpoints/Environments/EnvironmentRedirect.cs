using Microsoft.AspNetCore.Mvc;
using PreviewEnvironments.Application.Features.Abstractions;

namespace PreviewEnvironments.API.Endpoints.Environments;

public static class EnvironmentRedirect
{
    public const string Name = "EnvironmentRedirect";

    public static IEndpointRouteBuilder MapEnvironmentRedirect(this IEndpointRouteBuilder app)
    {
        _ = app.MapGet(
                Constants.EndPoints.Containers.EnvironmentRedirect,
                async ([FromRoute] string id, IRedirectFeature feature) =>
                {
                    Uri? redirectUri = await feature.GetRedirectUriAsync(id);

                    return redirectUri is null
                        ? Results.NotFound()
                        : Results.Redirect(redirectUri.ToString(), permanent: false);
                })
            .WithName(Name)
            .WithOpenApi()
            .Produces(StatusCodes.Status308PermanentRedirect);

        return app;
    }
}