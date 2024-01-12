namespace PreviewEnvironments.Application.Extensions;

internal static class EnvironmentHelper
{
    /// <summary>
    /// Gets the AzAccessToken from the environmental variables.
    /// </summary>
    /// <returns>
    /// The AzAccessToke or <see langword="null"/> if it was not found.
    /// </returns>
    public static string? GetAzAccessToken()
        => Environment.GetEnvironmentVariable(Constants.EnvVariables.AzAccessToken);
}
