namespace PreviewEnvironments.Application.Extensions;

internal static class EnvironmentHelper
{
    public static string? GetAzAccessToken()
    {
        string? token = Environment.GetEnvironmentVariable(Constants.EnvVariables.AzAccessToken);
        
        return token;
    }
}
